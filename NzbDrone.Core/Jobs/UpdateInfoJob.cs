﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class UpdateInfoJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ReferenceDataProvider _referenceDataProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public UpdateInfoJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                            ReferenceDataProvider referenceDataProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _referenceDataProvider = referenceDataProvider;
        }

        public UpdateInfoJob()
        {

        }

        public string Name
        {
            get { return "Update Episode Info"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(12); }
        }

        public virtual void Start(int targetId, int secondaryTargetId)
        {
            IList<Series> seriesToUpdate;
            if (targetId == 0)
            {
                seriesToUpdate = _seriesProvider.GetAllSeries().OrderBy(o => SortHelper.SkipArticles(o.Title)).ToList();
            }
            else
            {
                seriesToUpdate = new List<Series> { _seriesProvider.GetSeries(targetId) };
            }

            //Update any Daily Series in the DB with the IsDaily flag
            _referenceDataProvider.UpdateDailySeries();

            foreach (var series in seriesToUpdate)
            {
                try
                {
                    NotificationHelper.SendNotification("Updating {0}", series.Title);
                    _seriesProvider.UpdateSeriesInfo(series.SeriesId);
                    _episodeProvider.RefreshEpisodeInfo(series);
                    NotificationHelper.SendNotification("Update completed for {0}", series.Title);
                }

                catch(Exception ex)
                {
                    Logger.ErrorException("Failed to update episode info for series: " + series.Title, ex);
                }
                
            }
        }
    }
}