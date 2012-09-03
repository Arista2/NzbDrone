using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using NzbDrone.Services.Service.Providers;
using NzbDrone.Services.Service.Repository;

namespace NzbDrone.Services.Service.Controllers
{
    public class IdMappingController : Controller
    {
        private readonly IdMappingProvider _idMappingProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public IdMappingController(IdMappingProvider idMappingProvider)
        {
            _idMappingProvider = idMappingProvider;
        }

        [HttpGet]
        [OutputCache(CacheProfile = "Cache1HourVaryByTvdbId")]
        public JsonResult GetTvRageId(int tvDbId)
        {
            int id;

            try
            {
                id = _idMappingProvider.GetIdMapping(tvDbId).TvRageId;
            }
            catch(Exception ex)
            {
                logger.Warn("Unknown TVDB ID: {0}.", tvDbId);
                id = 0;
            }
            
            return Json(id, JsonRequestBehavior.AllowGet);
        }
    }
}