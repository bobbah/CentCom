using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentCom.Common.Data
{
    public sealed class NpgsqlDbContext : DatabaseContext
    {
        public NpgsqlDbContext(IConfiguration configuration) : base(configuration)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Configuration.GetSection("dbConfig")["connectionString"]);
        }
    }
}
