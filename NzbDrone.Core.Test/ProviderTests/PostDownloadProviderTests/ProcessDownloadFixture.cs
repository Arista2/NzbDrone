// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    public class ProcessDownloadFixture : TestBase
    {
        [Test]
        public void should_skip_if_folder_is_tagged_and_too_fresh()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var droppedFolder = new DirectoryInfo(TempFolder + "\\_test\\");
            droppedFolder.Create();

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);
        }

        [Test]
        public void should_continue_processing_if_folder_is_tagged_and_not_fresh()
        {
            var mocker = new AutoMoqer(MockBehavior.Loose);

            var droppedFolder = new DirectoryInfo(TempFolder + "\\_test\\");
            droppedFolder.Create();

            droppedFolder.LastWriteTime = DateTime.Now.AddMinutes(-2);

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<String>())).Returns<Series>(null).Verifiable();
            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }


        [Test]
        public void should_search_for_series_using_title_without_status()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Loose);
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\_unpack_The Office - S01E01 - Episode Title");

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns<Series>(null).Verifiable();

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void when_series_isnt_found_folder_should_be_tagged_as_unknown_series()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var taggedFolder = @"C:\Test\Unsorted TV\_UnknownSeries_The Office - S01E01 - Episode Title";

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns<Series>(null);
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void when_no_files_are_imported_folder_should_be_tagged_with_parse_error()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");

            var taggedFolder = @"C:\Test\Unsorted TV\_ParseError_The Office - S01E01 - Episode Title";

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(new List<EpisodeFile>());
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(100.Megabytes());


            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }


        [Test]
        public void when_no_file_are_imported_and_folder_size_isnt_small_enought_folder_should_be_tagged_unknown()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            var taggedFolder = PostDownloadProvider.GetFolderNameWithStatus(droppedFolder, PostDownloadStatusType.Unknown);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            //Act
            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskProvider>().Setup(s => s.MoveDirectory(droppedFolder.FullName, taggedFolder));
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(100.Megabytes());
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(fakeEpisodeFiles);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(true);

            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void when_files_are_imported_and_folder_is_small_enought_dir_should_be_deleted()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - Season 01");

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "The Office")
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(f => f.SeriesId = fakeSeries.SeriesId)
                .Build().ToList();

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("office")).Returns(fakeSeries);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(fakeEpisodeFiles);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true)).Returns(true);
            mocker.GetMock<DiskProvider>().Setup(s => s.GetDirectorySize(droppedFolder.FullName)).Returns(1.Megabytes());
            mocker.GetMock<DiskProvider>().Setup(s => s.DeleteFolder(droppedFolder.FullName, true));

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void all_imported_files_should_be_moved()
        {
            //Setup
            var mocker = new AutoMoqer(MockBehavior.Loose);
            var droppedFolder = new DirectoryInfo(TempFolder);

            var fakeSeries = Builder<Series>.CreateNew()
                .Build();

            var fakeEpisodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .Build().ToList();

            mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries(It.IsAny<string>())).Returns(fakeSeries);
            mocker.GetMock<DiskScanProvider>().Setup(s => s.Scan(fakeSeries, droppedFolder.FullName)).Returns(fakeEpisodeFiles);

            //Act
            mocker.Resolve<PostDownloadProvider>().ProcessDownload(droppedFolder);

            //Assert
            mocker.GetMock<DiskScanProvider>().Verify(c => c.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true),
                Times.Exactly(fakeEpisodeFiles.Count));
            mocker.VerifyAllMocks();
        }
    }
}