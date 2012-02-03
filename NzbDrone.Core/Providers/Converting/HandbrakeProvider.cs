﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Converting
{
    public class HandbrakeProvider
    {
        //Interacts with Handbrake
        private readonly ConfigProvider _configProvider;
        private Episode _currentEpisode;

        private Regex _processingRegex =
            new Regex(@"^(?:Encoding).+?(?:\,\s(?<percent>\d{1,3}\.\d{2})\s\%)(?:.+?ETA\s(?<hours>\d{2})h(?<minutes>\d{2})m(?<seconds>\d{2})s)?",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HandbrakeProvider(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public HandbrakeProvider()
        {
            
        }

        public virtual string ConvertFile(Episode episode)
        {
            _currentEpisode = episode;

            var outputFile = _configProvider.GetValue("iPodConvertDir", "");

            var handBrakePreset = _configProvider.GetValue("HandBrakePreset", "iPhone & iPod Touch");
            var handBrakeCommand = String.Format("-i \"{0}\" -o \"{1}\" --preset=\"{2}\"", episode.EpisodeFile.Path, outputFile, handBrakePreset);
            var handBrakeFile = @"C:\Program Files (x86)\Handbrake\HandBrakeCLI.exe";

            try
            {
                var process = new Process();
                process.StartInfo.FileName = handBrakeFile;
                process.StartInfo.Arguments = handBrakeCommand;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler(HandBrakeOutputDataReceived);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }

            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                return String.Empty;
            }

            return outputFile;
        }

        private void HandBrakeOutputDataReceived(object obj, DataReceivedEventArgs args)
        {
            throw new NotImplementedException();

            //args.Data contains the line writen

            var match = _processingRegex.Matches(args.Data);

            if (match.Count != 1)
                return;

            var episodeString = String.Format("{0} - {1}x{2:00}",
                _currentEpisode.Series.Title,
                _currentEpisode.SeasonNumber,
                _currentEpisode.EpisodeNumber);

            var percent = System.Convert.ToDecimal(match[0].Groups["percent"].Value);
            int hours;
            int minutes;
            int seconds;

            Int32.TryParse(match[0].Groups["hours"].Value, out hours);
            Int32.TryParse(match[0].Groups["minutes"].Value, out minutes);
            Int32.TryParse(match[0].Groups["seconds"].Value, out seconds);

            if (seconds > 0 || minutes > 0 || hours > 0)
            {
                var eta = DateTime.Now.Add(new TimeSpan(0, hours, minutes, seconds));
                NotificationHelper.SendNotification("Converting: {0}, {1}%. ETA: {2}", episodeString, percent, eta);
            }     

            else
                NotificationHelper.SendNotification("Converting: {0}, {1}%.", episodeString, percent);

            Console.WriteLine(args.Data);
        }
    }
}
