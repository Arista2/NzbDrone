using System;
using System.Data;
using System.Linq;
using Migrator.Framework;

namespace NzbDrone.Services.Service.Migrations
{
    [Migration(20120831)]
    public class Migration20120831 : Migration
    {
        public override void Up()
        {
            Database.AddTable("IdMappings", new Column("TvDbId", DbType.Int32, ColumnProperty.PrimaryKey),
                                            new Column("TvDbTitle", DbType.String, ColumnProperty.NotNull),
                                            new Column("TvRageId", DbType.Int32, ColumnProperty.NotNull),
                                            new Column("TvRageTitle", DbType.String, ColumnProperty.NotNull),
                                            new Column("FirstAired", DbType.DateTime, ColumnProperty.NotNull),
                                            new Column("Status", DbType.String, ColumnProperty.NotNull));
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}