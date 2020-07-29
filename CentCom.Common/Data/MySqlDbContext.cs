using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Net;

namespace CentCom.Common.Data
{
    public sealed class MySqlDbContext : DatabaseContext
    {
        public MySqlDbContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql(Configuration.GetSection("dbConfig")["connectionString"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MySql doesn't have support for IPAddress types
            modelBuilder.Entity<Ban>(e =>
            {
                e.Property(b => b.IP).HasConversion(
                    v => v.ToString(),
                    v => IPAddress.Parse(v));
            });
        }
    }
}
