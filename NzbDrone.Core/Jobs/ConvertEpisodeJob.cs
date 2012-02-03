﻿using System.Linq;
using System;
using Ninject;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Converting;

namespace NzbDrone.Core.Jobs
{
    public class ConvertEpisodeJob : IJob
    {
        private readonly HandbrakeProvider _handbrakeProvider;
        private readonly AtomicParsleyProvider _atomicParsleyProvider;
        private readonly EpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ConvertEpisodeJob(HandbrakeProvider handbrakeProvider, AtomicParsleyProvider atomicParsleyProvider,
                                    EpisodeProvider episodeProvider)
        {
            _handbrakeProvider = handbrakeProvider;
            _atomicParsleyProvider = atomicParsleyProvider;
            _episodeProvider = episodeProvider;
        }

        public string Name
        {
            get { return "Convert Episode"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public virtual void Start(int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            var episode = _episodeProvider.GetEpisode(targetId);
            NotificationHelper.SendNotification("Starting Conversion for {0}", episode);
            var outputFile = _handbrakeProvider.ConvertFile(episode);

            if (String.IsNullOrEmpty(outputFile))
                NotificationHelper.SendNotification("Conversion failed for {0}", episode);

            _atomicParsleyProvider.RunAtomicParsley(episode, outputFile);

            NotificationHelper.SendNotification("Conversion completed for {0}", episode);
        }
    }
}