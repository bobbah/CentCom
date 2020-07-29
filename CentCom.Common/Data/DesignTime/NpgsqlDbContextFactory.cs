using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentCom.Common.Data.DesignTime
{
    public class NpgsqlDbContextFactory : IDesignTimeDbContextFactory<NpgsqlDbContext>
    {
        public NpgsqlDbContext CreateDbContext(string[] args)
        {
            var conf = new Dictionary<string, string>
            {
                { "dbConfig:connectionString", "Host=127.0.0.1;Database=centcom_design;Username=centcom_parser;Password=spoof" },
                { "dbConfig:dbType", "Postgres" }
            };
            IConfiguration spoofedConfig = new ConfigurationBuilder().AddInMemoryCollection(conf).Build();
            return new NpgsqlDbContext(spoofedConfig);
        }
    }
}
