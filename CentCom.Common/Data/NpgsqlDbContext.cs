using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CentCom.Common.Data;

public sealed class NpgsqlDbContext : DatabaseContext
{
    public NpgsqlDbContext(IConfiguration configuration) : base(configuration)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(Configuration.GetSection("dbConfig")["connectionString"])
            .UseSnakeCaseNamingConvention();
    }
}