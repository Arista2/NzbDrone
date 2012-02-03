﻿using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Jobs
{
    public class AppUpdateJob : IJob
    {
        private readonly UpdateProvider _updateProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DiskProvider _diskProvider;
        private readonly HttpProvider _httpProvider;
        private readonly ProcessProvider _processProvider;
        private readonly ArchiveProvider _archiveProvider;
        private readonly ConfigFileProvider _configFileProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public AppUpdateJob(UpdateProvider updateProvider, EnviromentProvider enviromentProvider, DiskProvider diskProvider,
            HttpProvider httpProvider, ProcessProvider processProvider, ArchiveProvider archiveProvider, ConfigFileProvider configFileProvider)
        {
            _updateProvider = updateProvider;
            _enviromentProvider = enviromentProvider;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _processProvider = processProvider;
            _archiveProvider = archiveProvider;
            _configFileProvider = configFileProvider;
        }

        public string Name
        {
            get { return "Update Application Job"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(2); }
        }

        public virtual void Start(int targetId, int secondaryTargetId)
        {
            NotificationHelper.SendNotification("Checking for updates");

            var updatePackage = _updateProvider.GetAvilableUpdate();

            //No updates available
            if (updatePackage == null)
                return;

            var packageDestination = Path.Combine(_enviromentProvider.GetUpdateSandboxFolder(), updatePackage.FileName);

            if (_diskProvider.FolderExists(_enviromentProvider.GetUpdateSandboxFolder()))
            {
                logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(_enviromentProvider.GetUpdateSandboxFolder(), true);
            }

            logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            NotificationHelper.SendNotification("Downloading Update " + updatePackage.Version);
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            logger.Info("Extracting Update package");
            NotificationHelper.SendNotification("Extracting Update");
            _archiveProvider.ExtractArchive(packageDestination, _enviromentProvider.GetUpdateSandboxFolder());
            logger.Info("Update package extracted successfully");

            logger.Info("Preparing client");
            NotificationHelper.SendNotification("Preparing to start Update");
            _diskProvider.MoveDirectory(_enviromentProvider.GetUpdateClientFolder(), _enviromentProvider.GetUpdateSandboxFolder());


            logger.Info("Starting update client");
            var startInfo = new ProcessStartInfo
                                {
                FileName = _enviromentProvider.GetUpdateClientExePath(),
                Arguments = string.Format("{0} {1}", _enviromentProvider.NzbDroneProcessIdFromEnviroment, _configFileProvider.Guid)
            };

            _processProvider.Start(startInfo);
            NotificationHelper.SendNotification("Update in progress. NzbDrone will restart shortly.");
        }
    }
}