﻿using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110909)]
    public class Migration20110909 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", "Runtime", DbType.Int32, ColumnProperty.Null);
            Database.AddColumn("Series", "BannerUrl", DbType.String, ColumnProperty.Null);

            Database.AddTable("QualityTypes", new[]
                                            {
                                                new Column("QualityTypeId", DbType.Int32, ColumnProperty.PrimaryKey),
                                                new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                new Column("MinSize", DbType.Int32, ColumnProperty.NotNull),
                                                new Column("MaxSize", DbType.Int32, ColumnProperty.NotNull)
                                            });
        }
    }
}