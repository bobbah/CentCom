using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CentCom.Test;

public class DatabaseRegistrationTests
{
    [Test]
    [Arguments("Postgres", typeof(NpgsqlDbContext))]
    [Arguments("MySql", typeof(MySqlDbContext))]
    [Arguments("MariaDB", typeof(MariaDbContext))]
    public async Task RegistersConfiguredProvider(string provider, Type contextType)
    {
        var services = new ServiceCollection();
        services.AddCentComDatabase(Configuration(provider, "test connection"));

        await Assert.That(services.Single(x => x.ServiceType == typeof(DatabaseContext)).ImplementationType)
            .IsEqualTo(contextType);
    }

    [Test]
    [Arguments("Unknown", "test connection")]
    [Arguments("3", "test connection")]
    [Arguments("Postgres", "")]
    public async Task RejectsInvalidConfiguration(string provider, string connectionString)
    {
        await Assert.That(() => new ServiceCollection().AddCentComDatabase(
            Configuration(provider, connectionString))).Throws<InvalidOperationException>();
    }

    private static IConfiguration Configuration(string provider, string connectionString) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["dbConfig:dbType"] = provider,
            ["dbConfig:connectionString"] = connectionString
        }).Build();
}
