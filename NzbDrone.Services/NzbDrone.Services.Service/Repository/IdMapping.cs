using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository
{
    [TableName("IdMappings")]
    [PrimaryKey("TvDbId", autoIncrement = false)]
    public class IdMapping
    {
        public int TvDbId { get; set; }
        public string TvDbTitle { get; set; }
        public int TvRageId { get; set; }
        public string TvRageTitle { get; set; }
        public DateTime FirstAired { get; set; }
    }
}