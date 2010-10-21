﻿using System;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    class EpisodeFile
    {
        [SubSonicPrimaryKey]
        public virtual int FileId { get; set; }
        public int EpisodeId { get; set; }
        public string Path { get; set; }
        public QualityTypes Quality { get; set; }
        public bool Proper { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }

        [SubSonicToOneRelation]
        public virtual Episode Episode { get; set; }
    }
}