using System;
using CentCom.Common.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CentCom.Common.Configuration;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddCentComDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection("dbConfig");
        var connectionString = section["connectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Missing dbConfig:connectionString.");

        var rawType = section["dbType"];
        if (!Enum.TryParse<DbType>(rawType, true, out var dbType) || !Enum.IsDefined(dbType))
            throw new InvalidOperationException($"Unsupported dbConfig:dbType: '{rawType}'.");

        switch (dbType)
        {
            case DbType.Postgres:
                services.AddDbContext<DatabaseContext, NpgsqlDbContext>();
                break;
            case DbType.MariaDB:
                services.AddDbContext<DatabaseContext, MariaDbContext>();
                break;
            case DbType.MySql:
                services.AddDbContext<DatabaseContext, MySqlDbContext>();
                break;
        }

        return services;
    }
}
