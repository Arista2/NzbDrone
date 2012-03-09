﻿//http://stackoverflow.com/questions/210375/problems-reading-rss-with-c-and-net-3-5
//https://connect.microsoft.com/VisualStudio/feedback/details/325421/syndicationfeed-load-fails-to-parse-datetime-against-a-real-world-feeds-ie7-can-read

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;
using NLog;

namespace NzbDrone.Core.Providers.Indexer
{
    public class SyndicationFeedXmlReader : XmlTextReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly string[] rss20DateTimeHints = { "pubDate" };
        private static readonly string[] atom10DateTimeHints = { "updated", "published", "lastBuildDate" };
        private bool _isRss2DateTime;
        private bool _isAtomDateTime;

        private static readonly MethodInfo rss20FeedFormatterMethodInfo = typeof(Rss20FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo atom10FeedFormatterMethodInfo = typeof(Atom10FeedFormatter).GetMethod("DateFromString", BindingFlags.NonPublic | BindingFlags.Static);

        public SyndicationFeedXmlReader(Stream stream) : base(stream) { }

        public override bool IsStartElement(string localname, string ns)
        {
            _isRss2DateTime = rss20DateTimeHints.Contains(localname);
            _isAtomDateTime = atom10DateTimeHints.Contains(localname);

            return base.IsStartElement(localname, ns);
        }

        public override string ReadString()
        {
            var dateVal = base.ReadString();

            try
            {
                if (_isRss2DateTime)
                {
                    rss20FeedFormatterMethodInfo.Invoke(null, new object[] { dateVal, this });
                }
                if (_isAtomDateTime)
                {
                    atom10FeedFormatterMethodInfo.Invoke(new Atom10FeedFormatter(), new object[] { dateVal, this });
                }
            }
            catch (TargetInvocationException e)
            {
                DateTime parsedDate;

                if (!DateTime.TryParse(dateVal, new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate))
                {
                    parsedDate = DateTime.UtcNow;
                    logger.WarnException("Unable to parse Feed date " + dateVal, e);
                }

                dateVal = parsedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern);
            }

            return dateVal;
        }
    }
}
