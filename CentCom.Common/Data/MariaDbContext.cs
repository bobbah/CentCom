using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CentCom.Common.Data;

public class MariaDbContext : DatabaseContext
{
    public MariaDbContext(IConfiguration configuration) : base(configuration)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connStr = Configuration.GetSection("dbConfig")["connectionString"];
        var efcoreBuild =
            Configuration.GetSection("dbConfig")["efcoreBuild"] != null; // Used for building migrations
        options.UseMySql(connStr,
            efcoreBuild ? MariaDbServerVersion.LatestSupportedServerVersion : ServerVersion.AutoDetect(connStr));
    }
}