using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CentCom.Common.Data;

public abstract class DatabaseContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DatabaseContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public DbSet<Ban> Bans { get; set; }
    public DbSet<BanSource> BanSources { get; set; }
    public DbSet<JobBan> JobBans { get; set; }
    public DbSet<FlatBansVersion> FlatBansVersion { get; set; }
    public DbSet<CheckHistory> CheckHistory { get; set; }
    public DbSet<NotifiedFailure> NotifiedFailures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CKey).IsRequired().HasMaxLength(32);
            entity.Property(e => e.Source).IsRequired();
            entity.Property(e => e.BannedOn).IsRequired();
            entity.Property(e => e.Expires);
            entity.Property(e => e.BannedBy).IsRequired().HasMaxLength(32);
            entity.Property(e => e.UnbannedBy).HasMaxLength(32);
            entity.Property(e => e.BanType).IsRequired();
            entity.HasIndex(e => e.CKey);
            entity.HasMany(e => e.JobBans)
                .WithOne(b => b.BanNavigation)
                .HasForeignKey(b => b.BanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BanSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasMany(e => e.Bans)
                .WithOne(b => b.SourceNavigation)
                .HasForeignKey(b => b.Source)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Name).IsUnique(); // Force unique source names
        });

        modelBuilder.Entity<JobBan>(entity => { entity.HasKey(e => new { e.BanId, e.Job }); });

        modelBuilder.Entity<FlatBansVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.PerformedAt).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.HasIndex(e => new { e.Name, e.Version }).IsUnique();
        });

        modelBuilder.Entity<CheckHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Parser).IsRequired();
            entity.HasIndex(e => new { e.Parser, e.Started });
        });

        modelBuilder.Entity<NotifiedFailure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasOne(e => e.CheckHistory)
                .WithOne(e => e.Notification)
                .HasForeignKey<NotifiedFailure>(e => e.CheckHistoryId);
        });
    }

    public async Task<bool> Migrate(CancellationToken cancellationToken)
    {
        var migrations = await Database.GetAppliedMigrationsAsync(cancellationToken);
        var wasEmpty = !migrations.Any();
        if (wasEmpty || (await Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await Database.MigrateAsync(cancellationToken);
        }

        return wasEmpty;
    }
}