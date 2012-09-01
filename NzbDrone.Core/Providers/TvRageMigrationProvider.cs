using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Ninject;

namespace NzbDrone.Core.Providers
{
    public class TvRageMigrationProvider
    {
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public TvRageMigrationProvider(SeriesProvider seriesProvider)
        {
            _seriesProvider = seriesProvider;
        }

        public TvRageMigrationProvider()
        {
        }

        public virtual void MapIds()
        {
            //Lets map TVRage to TheTVDB
        }
    }
}
