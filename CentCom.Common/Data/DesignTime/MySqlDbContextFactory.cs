using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CentCom.Common.Data.DesignTime;

class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    public MySqlDbContext CreateDbContext(string[] args)
    {
        var conf = new Dictionary<string, string>
        {
            { "dbConfig:connectionString", "Host=127.0.0.1;Database=centcom_design;Username=centcom_parser;Password=spoof" },
            { "dbConfig:dbType", "MySql" },
            { "dbConfig:efcoreBuild", "any-value" }
        };
        IConfiguration spoofedConfig = new ConfigurationBuilder().AddInMemoryCollection(conf).Build();
        return new MySqlDbContext(spoofedConfig);
    }
}