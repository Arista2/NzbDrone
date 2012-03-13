// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvRageProviderTest : CoreTest
    {
        private HttpProvider httpProvider;
        private TvRageProvider tvRageProvider;

        [SetUp]
        public void Setup()
        {
            var kernel = new StandardKernel();

            httpProvider = kernel.Get<HttpProvider>();
            tvRageProvider = kernel.Get<TvRageProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            ExceptionVerification.MarkInconclusive(typeof(TvdbNotAvailableException));
        }

        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        public void Valid_search_should_return_valid_results(string title)
        {
            var result = tvRageProvider.SearchSeries(title);

            result.Should().NotBeEmpty();
            result[0].Name.Should().Be(title);
        }

        [Test]
        public void Search_for_guid_should_return_empty_result()
        {
            //act
            var result = tvRageProvider.SearchSeries(Guid.NewGuid().ToString());

            //assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetEpisodes_should_unique_episodes()
        {
            //act
            var result = tvRageProvider.GetEpisodes(3506);//Family guy

            //Asserts that when episodes are grouped by Season/Episode each group contains maximum of
            //one item.
            result.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);
        }

        [Test]
        public void get_series_should_return_from_tvRage()
        {
            //act
            var result = tvRageProvider.GetSeries(3506);//Family guy
            result.Name.Should().Be("Family Guy");

        }

        //[Test]
        //public void American_dad_fix()
        //{
        //    //act
        //    var result = httpProvider.GetSeries(73141, true);

        //    var seasonsNumbers = result.Episodes.Select(e => e.SeasonNumber)
        //        .Distinct().ToList();

        //    var seasons = new Dictionary<int, List<TvdbEpisode>>(seasonsNumbers.Count);

        //    foreach (var season in seasonsNumbers)
        //    {
        //        seasons.Add(season, result.Episodes.Where(e => e.SeasonNumber == season).ToList());
        //    }

        //    foreach (var episode in result.Episodes)
        //    {
        //        Console.WriteLine(episode);
        //    }

        //    //assert
        //    seasonsNumbers.Should().HaveCount(8);
        //    seasons[1].Should().HaveCount(23);
        //    seasons[2].Should().HaveCount(19);
        //    seasons[3].Should().HaveCount(16);
        //    seasons[4].Should().HaveCount(20);
        //    seasons[5].Should().HaveCount(18);
        //    seasons[6].Should().HaveCount(19);

        //    foreach (var season in seasons)
        //    {
        //        season.Value.Should().OnlyHaveUniqueItems("Season {0}", season.Key);
        //    }

        //    //Make sure no episode number is skipped
        //    foreach (var season in seasons)
        //    {
        //        for (int i = 1; i < season.Value.Count; i++)
        //        {
        //            //Skip specials, because someone decided 1,3,4,6,7,21 is how you count...
        //            if (season.Key == 0)
        //                continue;

        //            season.Value.Should().Contain(c => c.EpisodeNumber == i, "Can't find Episode S{0:00}E{1:00}",
        //                                    season.Value[0].SeasonNumber, i);
        //        }
        //    }
        //}
    }
}