using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CentCom.Common.Data
{
    public sealed class MySqlDbContext : DatabaseContext
    {
        public MySqlDbContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connStr = Configuration.GetSection("dbConfig")["connectionString"];
            options.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
