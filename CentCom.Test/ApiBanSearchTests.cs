using CentCom.API.Controllers;
using CentCom.API.Services.Implemented;
using CentCom.Common.Data;
using CentCom.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CentCom.Test;

public class ApiBanSearchTests
{
    [Test]
    public async Task SearchPaginatesByLatestBanAndCKey()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        for (var i = 0; i < 27; i++)
            db.Bans.Add(CreateBan(source, $"prefixfoo{i:D3}"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new BanService(db);

        var first = await service.SearchSummariesForKeyAsync("foo", 1);
        var second = await service.SearchSummariesForKeyAsync("foo", 2);

        await Assert.That(first.Data.Count).IsEqualTo(25);
        await Assert.That(first.HasNextPage).IsTrue();
        await Assert.That(first.Data[0].CKey).IsEqualTo("prefixfoo000");
        await Assert.That(second.Data.Select(x => x.CKey).ToArray())
            .IsEquivalentTo(new[] { "prefixfoo025", "prefixfoo026" });
        await Assert.That(second.HasNextPage).IsFalse();
    }

    [Test]
    public async Task SearchFiltersBeforeGroupingAndCountsMatchingBans()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.AddRange(
            CreateBan(source, "prefixfoo"),
            CreateBan(source, "prefixfoo"),
            CreateBan(source, "unrelated"));
        await db.SaveChangesAsync();

        var result = await new BanService(db).SearchSummariesForKeyAsync("foo", 1);

        await Assert.That(result.Data.Count).IsEqualTo(1);
        await Assert.That(result.Data[0].CKey).IsEqualTo("prefixfoo");
        await Assert.That(result.Data[0].ServerBans).IsEqualTo(2);
    }

    [Test]
    public async Task ProviderQueriesUseIndexedSubstringCandidates()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["dbConfig:connectionString"] = "Server=localhost;Database=centcom;User=centcom;Password=placeholder",
            ["dbConfig:efcoreBuild"] = "true"
        }).Build();
        foreach (var context in new DatabaseContext[]
                 {
                     new MySqlDbContext(configuration),
                     new MariaDbContext(configuration)
                 })
        {
            await using (context)
            {
                var sql = BanSearchQuery.ForCKeySubstring(context, "foo").ToQueryString();
                await Assert.That(sql).Contains("BanCKeyGrams");
                await Assert.That(sql).Contains("EXISTS");
                await Assert.That(sql).Contains("LIKE");

                var shortSearch = BanSearchQuery.ForCKeySubstring(context, "fo").ToQueryString();
                await Assert.That(shortSearch.Contains("BanCKeyGrams")).IsFalse();
            }
        }

        var postgresConfiguration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["dbConfig:connectionString"] = "Host=localhost;Database=centcom;Username=centcom;Password=placeholder"
            }).Build();
        await using var postgres = new NpgsqlDbContext(postgresConfiguration);
        var postgresSql = BanSearchQuery.ForCKeySubstring(postgres, "foo").ToQueryString();
        await Assert.That(postgresSql).Contains("LIKE");
        await Assert.That(postgresSql).Contains("lower");
        await Assert.That(postgresSql.Contains("BanCKeyGrams")).IsFalse();
    }

    [Test]
    public async Task MissingBanReturnsNullAndControllerReturnsNotFound()
    {
        await using var db = CreateContext();
        var service = new BanService(db);
        await Assert.That(await service.GetBanAsync(999)).IsNull();

        var controller = new BanController(service, new BanSourceService(db));
        var response = await controller.GetBan(999);
        await Assert.That(response is NotFoundObjectResult).IsTrue();
    }

    [Test]
    public async Task ExistingBanReturnsData()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.Add(CreateBan(source, "player"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new BanService(db).GetBanAsync(await db.Bans.Select(x => x.Id).SingleAsync());
        await Assert.That(result.CKey).IsEqualTo("player");
        await Assert.That(result.SourceName).IsEqualTo("Test");
    }

    private static TestDbContext CreateContext() =>
        new(new ConfigurationBuilder().Build(), Guid.NewGuid().ToString());

    private static async Task<BanSource> SeedSource(TestDbContext db)
    {
        var source = new BanSource { Name = "test", Display = "Test" };
        db.BanSources.Add(source);
        await db.SaveChangesAsync();
        return source;
    }

    private static Ban CreateBan(BanSource source, string key) => new()
    {
        Source = source.Id,
        SourceNavigation = source,
        BanType = BanType.Server,
        CKey = key,
        BannedBy = "admin",
        Reason = "test",
        BannedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private sealed class TestDbContext(IConfiguration configuration, string name) : DatabaseContext(configuration)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseInMemoryDatabase(name);
    }
}
