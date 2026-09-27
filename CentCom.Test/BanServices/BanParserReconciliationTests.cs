using System.Reflection;
using CentCom.Common.Data;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.BanSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;

namespace CentCom.Test.BanServices;

public class BanParserReconciliationTests
{
    [Test]
    public async Task EmptyFullRefreshPreservesSingleStoredBan()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.Add(CreateBan(source, "existing"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var parser = new TestBanParser(db, []);
        await Assert.That(() => parser.Execute(JobContext(completeRefresh: true))).Throws<JobExecutionException>();

        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(1);
        var failure = await db.CheckHistory.SingleAsync();
        await Assert.That(failure.Success).IsFalse();
        await Assert.That(failure.CompleteRefresh).IsTrue();
    }

    [Test]
    public async Task EmptyInitialFetchIsReportedAsFailure()
    {
        await using var db = CreateContext();
        await SeedSource(db);

        var parser = new TestBanParser(db, []);
        await Assert.That(() => parser.Execute(JobContext(completeRefresh: false))).Throws<JobExecutionException>();

        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(0);
        await Assert.That((await db.CheckHistory.SingleAsync()).Success).IsFalse();
    }

    [Test]
    public async Task EmptyIncrementalFetchLeavesStoredBansUntouched()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.Add(CreateBan(source, "existing"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new TestBanParser(db, []).Execute(JobContext(completeRefresh: false));

        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(1);
        var history = await db.CheckHistory.SingleAsync();
        await Assert.That(history.Success).IsTrue();
        await Assert.That(history.CompleteRefresh).IsFalse();
    }

    [Test]
    public async Task AllInvalidFetchedBansCannotTriggerDeletion()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.Add(CreateBan(source, "existing"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var invalid = CreateBan(source, "invalid");
        invalid.CKey = null!;
        var parser = new TestBanParser(db, [invalid]);
        await Assert.That(() => parser.Execute(JobContext(completeRefresh: true))).Throws<JobExecutionException>();

        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(1);
        await Assert.That((await db.CheckHistory.SingleAsync()).Success).IsFalse();
    }

    [Test]
    public async Task NonemptyFullRefreshMirrorsRemovedBans()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        db.Bans.AddRange(CreateBan(source, "kept"), CreateBan(source, "removed-1"),
            CreateBan(source, "removed-2"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var parser = new TestBanParser(db, [CreateBan(source, "kept")]);
        await parser.Execute(JobContext(completeRefresh: true));

        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(1);
        await Assert.That((await db.Bans.SingleAsync()).BanID).IsEqualTo("kept");
        var history = await db.CheckHistory.SingleAsync();
        await Assert.That(history.Deleted).IsEqualTo(2);
        await Assert.That(history.Success).IsTrue();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExistingJobBanReconcilesAddedAndRemovedRoles(bool completeRefresh)
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        var stored = CreateBan(source, "job");
        stored.BanType = BanType.Job;
        stored.AddJobRange(["assistant", "cargo technician"]);
        db.Bans.Add(stored);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var fetched = CreateBan(source, "job");
        fetched.BanType = BanType.Job;
        fetched.AddJobRange(["cargo technician", "engineer"]);
        await new TestBanParser(db, [fetched]).Execute(JobContext(completeRefresh));

        db.ChangeTracker.Clear();
        var updated = await db.Bans.Include(x => x.JobBans).SingleAsync();
        await Assert.That(updated.JobBans.Select(x => x.Job).Order().ToArray())
            .IsEquivalentTo(new[] { "cargo technician", "engineer" });
        await Assert.That((await db.CheckHistory.SingleAsync()).Updated).IsEqualTo(1);
    }

    [Test]
    public async Task ExistingJobBanBecomingServerBanClearsRoles()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        var stored = CreateBan(source, "changed-type");
        stored.BanType = BanType.Job;
        stored.AddJob("engineer");
        db.Bans.Add(stored);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new TestBanParser(db, [CreateBan(source, "changed-type")])
            .Execute(JobContext(completeRefresh: false));

        db.ChangeTracker.Clear();
        var updated = await db.Bans.Include(x => x.JobBans).SingleAsync();
        await Assert.That(updated.BanType).IsEqualTo(BanType.Server);
        await Assert.That(updated.JobBans.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ConfigurationFailureIsRecordedInHistory()
    {
        await using var db = CreateContext();
        var parser = new TestBanParser(db, [], new InvalidOperationException("Invalid parser setup"));

        await Assert.That(() => parser.Execute(JobContext(completeRefresh: true))).Throws<JobExecutionException>();

        var failure = await db.CheckHistory.SingleAsync();
        await Assert.That(failure.Success).IsFalse();
        await Assert.That(failure.ExceptionDetailed).Contains("Invalid parser setup");
    }

    [Test]
    public async Task IncrementalRefreshTracksOnlyMatchingStoredBans()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        for (var i = 0; i < 200; i++)
            db.Bans.Add(CreateBan(source, $"ban-{i}"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var fetched = CreateBan(source, "ban-199");
        fetched.Reason = "updated";
        await new TestBanParser(db, [fetched]).Execute(JobContext(completeRefresh: false));

        await Assert.That(db.ChangeTracker.Entries<Ban>().Count()).IsEqualTo(1);
        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(200);
        await Assert.That((await db.Bans.SingleAsync(x => x.BanID == "ban-199")).Reason).IsEqualTo("updated");
    }

    [Test]
    public async Task IncrementalRefreshWithoutIdsTracksOnlyFetchedDateRange()
    {
        await using var db = CreateContext();
        var source = await SeedSource(db);
        var oldest = DateTime.UtcNow.AddDays(-30);
        for (var i = 0; i < 200; i++)
        {
            var ban = CreateBan(source, $"ban-{i}");
            ban.BannedOn = oldest.AddHours(i);
            db.Bans.Add(ban);
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var fetched = CreateBan(source, "ban-199");
        fetched.BannedOn = oldest.AddHours(199);
        fetched.Reason = "updated";
        await new TestBanParser(db, [fetched], supportsBanIds: false)
            .Execute(JobContext(completeRefresh: false));

        await Assert.That(db.ChangeTracker.Entries<Ban>().Count()).IsEqualTo(1);
        await Assert.That(await db.Bans.CountAsync()).IsEqualTo(200);
        await Assert.That((await db.Bans.SingleAsync(x => x.BanID == "ban-199")).Reason).IsEqualTo("updated");
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

    private static Ban CreateBan(BanSource source, string id) => new()
    {
        Source = source.Id,
        SourceNavigation = source,
        BanID = id,
        BanType = BanType.Server,
        CKey = "test",
        BannedBy = "admin",
        Reason = "test",
        BannedOn = DateTime.UtcNow
    };

    private static IJobExecutionContext JobContext(bool completeRefresh)
    {
        var context = DispatchProxy.Create<IJobExecutionContext, JobContextProxy>();
        var proxy = (JobContextProxy)context;
        proxy.Job = JobBuilder.Create<TestBanParser>().WithIdentity("test").Build();
        proxy.Data = new JobDataMap { ["completeRefresh"] = completeRefresh };
        return context;
    }

    public class JobContextProxy : DispatchProxy
    {
        public IJobDetail Job { get; set; } = null!;
        public JobDataMap Data { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
        {
            "get_JobDetail" => Job,
            "get_MergedJobDataMap" => Data,
            "get_FireTimeUtc" => DateTimeOffset.UtcNow,
            _ => throw new NotSupportedException(targetMethod?.Name)
        };
    }

    private sealed class TestBanParser(DatabaseContext db, List<Ban> bans, Exception? configureError = null,
        bool supportsBanIds = true)
        : BanParser(db, NullLogger<BanParser>.Instance)
    {
        protected override Dictionary<string, BanSource> Sources => new()
        {
            ["test"] = new BanSource { Name = "test", Display = "Test" }
        };

        protected override bool SourceSupportsBanIDs => supportsBanIds;
        protected override string Name => "test";

        protected override Task Configure(IJobExecutionContext context) =>
            configureError is null ? Task.CompletedTask : Task.FromException(configureError);

        public override Task<List<Ban>> FetchNewBansAsync() => Task.FromResult(bans);
        public override Task<List<Ban>> FetchAllBansAsync() => Task.FromResult(bans);
    }

    private sealed class TestDbContext(IConfiguration configuration, string name) : DatabaseContext(configuration)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseInMemoryDatabase(name);
    }
}
