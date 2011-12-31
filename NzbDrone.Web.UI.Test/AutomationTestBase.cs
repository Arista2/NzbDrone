﻿using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Web.UI.Automation
{
    public abstract class AutomationTestBase
    {
        static readonly EnviromentProvider enviromentProvider = new EnviromentProvider();
        private static readonly string testFolder;

        public string AppUrl
        {
            get
            {
                return "http://localhost:8989";
            }
        }


        public RemoteWebDriver Driver { get; private set; }

        static AutomationTestBase()
        {
            CleanBinFolder();
            testFolder = CreatePackage();
            StartNzbDrone();
        }

        [SetUp]
        public void AutomationSetup()
        {
            
        }

        [TearDown]
        public void AutomationTearDown()
        {
            

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Screenshots"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Screenshots");
            }

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*__*.png").Select(c => new FileInfo(c)))
            {
                File.Copy(file.FullName, Directory.GetCurrentDirectory() + "\\Screenshots\\" + file.Name, true);
                file.Delete();
            }
        }


        [TestFixtureSetUp]
        public void AutomationTestFixtureSetup()
        {
            StopNzbDrone();
            ResetUserData();
            StartNzbDrone();
            Driver = new FirefoxDriver();
        }

        [TestFixtureTearDown]
        public void AutomationTestFixtureTearDown()
        {
            Driver.Close();
            StopNzbDrone();

            File.Copy(Path.Combine(testFolder, "nzbdrone.log"), Path.Combine(Directory.GetCurrentDirectory(), "nzbdrone.log"), true);
        }


        protected void CaptureScreen()
        {
            var method = new StackFrame(1).GetMethod().Name;

            var fileName = String.Format("{0}__{1}.png", GetType().Name, method);

            ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(fileName, ImageFormat.Png);
        }

        private void ResetUserData()
        {
            var appDataPath = Path.Combine(testFolder, "NzbDrone.Web", "app_data");

            if (Directory.Exists(appDataPath))
                Directory.Delete(appDataPath, true);

        }


        private static void CleanBinFolder()
        {
            var folderName = "Debug";

            if (EnviromentProvider.IsDebug)
            {
                folderName = "Release";
            }

            var dirs = Directory.GetDirectories(enviromentProvider.ApplicationPath, folderName, SearchOption.AllDirectories);


            foreach (var dir in dirs)
            {
                Directory.Delete(dir, true);
            }

        }

        static void StartNzbDrone()
        {

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(testFolder, "nzbdrone.exe"),
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var nzbDroneProcess = new Process
                                      {
                                          StartInfo = startInfo
                                      };
            nzbDroneProcess.OutputDataReceived +=
                    delegate(object o, DataReceivedEventArgs args)
                    {
                        Console.WriteLine(args.Data);
                    };

            nzbDroneProcess.Start();
        }



        public static void StopNzbDrone()
        {
            foreach (var process in Process.GetProcessesByName("nzbdrone"))
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        private static string CreatePackage()
        {
            Console.WriteLine("Creating NzbDrone Package");

            StopNzbDrone();

            var rootDirectory = new DirectoryInfo(enviromentProvider.ApplicationPath);

            if (rootDirectory.GetDirectories("_rawPackage").Any())
            {
                rootDirectory.GetDirectories("_rawPackage").ToList().ForEach(c => c.Delete(true));
            }

            var startInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(rootDirectory.FullName, "package.bat"),
                                    WorkingDirectory = rootDirectory.FullName
                                };

            Process.Start(startInfo).WaitForExit();

            var testFolder = Path.Combine(enviromentProvider.SystemTemp, "NzbDroneAutomation");

            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }

            Directory.Move(Path.Combine(rootDirectory.FullName, "_rawPackage", "nzbdrone"), testFolder);



            return testFolder;
        }
    }
}