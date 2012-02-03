﻿using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonSearchJobTest : CoreTest
    {
        [Test]
        public void SeasonSearch_full_season_success()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(1, 1)).Returns(true);

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(1, 1);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(1, 1), Times.Never());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<int>(), 0), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_success()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .Build();

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(1, 1)).Returns(false);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(1, 1))
                .Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(1, 1);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<int>(), 0), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_failure()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .Build();

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(1, 1)).Returns(false);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(1, 1))
                .Returns(new List<int>{1});

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(c => c.Start(It.IsAny<int>(), 0)).Verifiable();

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(1, 1);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<int>(), 0), Times.Exactly(4));
        }

        [Test]
        public void SeasonSearch_should_not_search_for_episodes_that_havent_aired_yet_or_air_tomorrow()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .TheLast(2)
                .With(e => e.AirDate = DateTime.Today.AddDays(2))
                .Build();

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(1, 1)).Returns(false);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(1, 1))
                .Returns(new List<int>());

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(c => c.Start(It.IsAny<int>(), 0)).Verifiable();

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(1, 1);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(1, 1), Times.Once());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<int>(), 0), Times.Exactly(3));
        }
    }
}