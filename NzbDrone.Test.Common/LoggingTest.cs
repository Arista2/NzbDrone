using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Common;

namespace NzbDrone.Test.Common
{
    public abstract class LoggingTest
    {
        protected static void InitLogging()
        {
            if (LogManager.Configuration == null || LogManager.Configuration is XmlLoggingConfiguration)
            {
                LogManager.Configuration = new LoggingConfiguration();
                LogConfiguration.RegisterConsoleLogger(LogLevel.Trace);
                LogConfiguration.RegisterUdpLogger();

                RegisterExceptionVerification();
                LogConfiguration.Reload();
            }
        }

        private static void RegisterExceptionVerification()
        {
            var exceptionVerification = new ExceptionVerification();
            LogManager.Configuration.AddTarget("ExceptionVerification", exceptionVerification);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, exceptionVerification));
        }

        [SetUp]
        public void LoggingTestSetup()
        {
            InitLogging();
            ExceptionVerification.Reset();
        }

        [TearDown]
        public void LoggingDownBase()
        {
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }
    }
}
