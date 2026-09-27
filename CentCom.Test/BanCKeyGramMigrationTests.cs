using CentCom.Common.Data;
using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using MariaMigration = CentCom.Common.Migrations.MariaDb.AddCKeySubstringIndex;
using MariaLookupMigration = CentCom.Common.Migrations.MariaDb.AddBanSourceLookupIndexes;
using MySqlMigration = CentCom.Common.Migrations.MySql.AddCKeySubstringIndex;
using MySqlLookupMigration = CentCom.Common.Migrations.MySql.AddBanSourceLookupIndexes;
using PostgresMigration = CentCom.Common.Migrations.Postgres.AddCKeySubstringIndex;
using PostgresLookupMigration = CentCom.Common.Migrations.Postgres.AddBanSourceLookupIndexes;

namespace CentCom.Test;

public class BanCKeyGramMigrationTests
{
    [Test]
    public async Task MySqlAndMariaDbModelPostingTable()
    {
        foreach (var context in new DatabaseContext[]
                 {
                     new MySqlDbContext(Configuration()),
                     new MariaDbContext(Configuration())
                 })
        {
            await using (context)
            {
                var gram = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(BanCKeyGram))!;
                var primaryKey = gram.FindPrimaryKey()!;
                var foreignKey = gram.GetForeignKeys().Single();

                await Assert.That(gram.GetTableName()).IsEqualTo("BanCKeyGrams");
                await Assert.That(primaryKey.Properties.Select(p => p.Name))
                    .IsEquivalentTo(new[] { nameof(BanCKeyGram.Gram), nameof(BanCKeyGram.BanId) });
                await Assert.That(gram.FindProperty(nameof(BanCKeyGram.Gram))!.GetCollation())
                    .IsEqualTo("ascii_bin");
                await Assert.That(foreignKey.DeleteBehavior).IsEqualTo(DeleteBehavior.Cascade);
                await Assert.That(gram.GetIndexes().Any(i => i.Properties.Single().Name == nameof(BanCKeyGram.BanId)))
                    .IsTrue();
            }
        }
    }

    [Test]
    public async Task PostgresDoesNotModelPostingTable()
    {
        await using var context = new NpgsqlDbContext(Configuration());
        await Assert.That(context.Model.FindEntityType(typeof(BanCKeyGram))).IsNull();
    }

    [Test]
    public async Task MigrationsCreateProviderSpecificIndexes()
    {
        foreach (var migration in new Microsoft.EntityFrameworkCore.Migrations.Migration[]
                 {
                     new MySqlMigration(), new MariaMigration()
                 })
        {
            var sql = migration.UpOperations.OfType<SqlOperation>().Select(o => o.Sql).ToArray();
            await Assert.That(migration.UpOperations.OfType<CreateTableOperation>()
                .Any(o => o.Name == "BanCKeyGrams")).IsTrue();
            await Assert.That(sql.Any(s => s.Contains("CREATE TRIGGER `trg_bans_ckey_grams_insert`"))).IsTrue();
            await Assert.That(sql.Any(s => s.Contains("CREATE TRIGGER `trg_bans_ckey_grams_update`"))).IsTrue();
            await Assert.That(sql.Any(s => s.Contains("SELECT DISTINCT b.`Id`"))).IsTrue();
            await Assert.That(migration.UpOperations.OfType<AlterColumnOperation>().Any()).IsFalse();
            await Assert.That(migration.DownOperations.OfType<SqlOperation>()
                .Count(o => o.Sql.StartsWith("DROP TRIGGER"))).IsEqualTo(2);
            await Assert.That(migration.DownOperations.OfType<DropTableOperation>()
                .Any(o => o.Name == "BanCKeyGrams")).IsTrue();
        }

        var postgresSql = new PostgresMigration().UpOperations.OfType<SqlOperation>()
            .Select(o => o.Sql).ToArray();
        await Assert.That(postgresSql.Any(s => s.Contains("CREATE EXTENSION IF NOT EXISTS pg_trgm"))).IsTrue();
        await Assert.That(postgresSql.Any(s => s.Contains("lower(c_key) gin_trgm_ops"))).IsTrue();
        await Assert.That(new PostgresMigration().DownOperations.OfType<SqlOperation>()
            .Any(o => o.Sql.Contains("DROP INDEX ix_bans_lower_c_key_trgm"))).IsTrue();
    }

    [Test]
    public async Task SourceLookupMigrationsOnlyAddIndexes()
    {
        foreach (var migration in new Microsoft.EntityFrameworkCore.Migrations.Migration[]
                 {
                     new MySqlLookupMigration(), new MariaLookupMigration(), new PostgresLookupMigration()
                 })
        {
            var indexes = migration.UpOperations.OfType<CreateIndexOperation>().ToArray();
            await Assert.That(indexes.Length).IsEqualTo(2);
            await Assert.That(indexes.Any(i => i.Columns.SequenceEqual(new[]
                { "Source", "BanID" }) || i.Columns.SequenceEqual(new[] { "source", "ban_id" }))).IsTrue();
            await Assert.That(indexes.Any(i => i.Columns.SequenceEqual(new[]
                { "Source", "BannedOn" }) || i.Columns.SequenceEqual(new[] { "source", "banned_on" }))).IsTrue();
            await Assert.That(migration.UpOperations.Count).IsEqualTo(2);
        }

        foreach (var migration in new Microsoft.EntityFrameworkCore.Migrations.Migration[]
                 {
                     new MySqlLookupMigration(), new MariaLookupMigration()
                 })
        {
            var idIndex = migration.UpOperations.OfType<CreateIndexOperation>()
                .Single(i => i.Columns.Contains("BanID"));
            await Assert.That((int[])idIndex["MySql:IndexPrefixLength"]!).IsEquivalentTo(new[] { 0, 128 });
        }
    }

    [Test]
    public async Task SourceLookupModelRetainsOriginalBanIdColumnAndSourceIndex()
    {
        foreach (var context in new DatabaseContext[]
                 {
                     new MySqlDbContext(Configuration()),
                     new MariaDbContext(Configuration()),
                     new NpgsqlDbContext(Configuration())
                 })
        {
            await using (context)
            {
                var ban = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Ban))!;
                await Assert.That(ban.GetIndexes().Any(i => i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { nameof(Ban.Source), nameof(Ban.BanID) }))).IsTrue();
                await Assert.That(ban.GetIndexes().Any(i => i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { nameof(Ban.Source), nameof(Ban.BannedOn) }))).IsTrue();
                await Assert.That(ban.GetIndexes().Any(i => i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { nameof(Ban.Source) }))).IsTrue();
                if (context is MySqlDbContext or MariaDbContext)
                {
                    await Assert.That(ban.FindProperty(nameof(Ban.BanID))!.GetColumnType())
                        .IsEqualTo("longtext");
                }
            }
        }
    }

    private static IConfiguration Configuration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["dbConfig:connectionString"] = "Server=localhost;Database=centcom;User=centcom;Password=unused",
            ["dbConfig:efcoreBuild"] = "true"
        }).Build();
}
