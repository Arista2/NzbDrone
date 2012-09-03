using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    class UnknownTvRageIdException : Exception
    {
        public int TvDbId { get; private set; }

        public UnknownTvRageIdException(string message, int tvDbId) : base(message)
        {
            TvDbId = tvDbId;
        }
    }
}
