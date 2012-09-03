using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Services.Service.Repository;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Providers
{
    public class IdMappingProvider
    {
        private readonly IDatabase _database;

        public IdMappingProvider(IDatabase database)
        {
            _database = database;
        }

        public IdMapping GetIdMapping(int tvDbId)
        {
            return _database.SingleById<IdMapping>(tvDbId);
        }
    }
}