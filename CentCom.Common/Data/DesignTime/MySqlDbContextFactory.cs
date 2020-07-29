using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CentCom.Common.Data.DesignTime
{
    class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
    {
        public MySqlDbContext CreateDbContext(string[] args)
        {
            var conf = new Dictionary<string, string>
            {
                { "dbConfig:connectionString", "Host=127.0.0.1;Database=centcom_design;Username=centcom_parser;Password=spoof" },
                { "dbConfig:dbType", "Postgres" }
            };
            IConfiguration spoofedConfig = new ConfigurationBuilder().AddInMemoryCollection(conf).Build();
            return new MySqlDbContext(spoofedConfig);
        }
    }
}
