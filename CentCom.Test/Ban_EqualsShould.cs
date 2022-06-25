using System;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Equality;
using Xunit;

namespace CentCom.Test;

public class Ban_EqualsShould
{
    [Fact]
    public void Equals_SameBanDifferentID_ReturnTrue()
    {
        var source = new BanSource
        {
            Display = "test source",
            Name = "test",
            RoleplayLevel = RoleplayLevel.Medium,
            Id = 3
        };

        var banA = new Ban
        {
            Id = 12,
            CKey = "test",
            BannedOn = DateTime.MinValue,
            BannedBy = "tester",
            BanType = BanType.Server,
            Reason = "what a great test",
            Source = source.Id,
            SourceNavigation = source
        };

        var banB = new Ban
        {
            Id = 0,
            CKey = "test",
            BannedOn = DateTime.MinValue,
            BannedBy = "tester",
            BanType = BanType.Server,
            Reason = "what a great test",
            Source = source.Id,
            SourceNavigation = source
        };

        var comparer = BanEqualityComparer.Instance;
        Assert.True(comparer.Equals(banA, banB), "Two bans equal by internal values should be equal");
        Assert.True(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Two bans equal by internal values should have equal hashcodes");
    }

    [Fact]
    public void Equals_SameBanDifferentIDDifferentSource_ReturnFalse()
    {
        var sourceA = new BanSource
        {
            Display = "test source A",
            Name = "testA",
            RoleplayLevel = RoleplayLevel.Medium,
            Id = 3
        };

        var sourceB = new BanSource
        {
            Display = "test source B",
            Name = "testB",
            RoleplayLevel = RoleplayLevel.Medium,
            Id = 4
        };

        var banA = new Ban
        {
            Id = 12,
            CKey = "test",
            BannedOn = DateTime.MinValue,
            BannedBy = "tester",
            BanType = BanType.Server,
            Reason = "what a great test",
            Source = sourceA.Id,
            SourceNavigation = sourceA
        };

        var banB = new Ban
        {
            Id = 0,
            CKey = "test",
            BannedOn = DateTime.MinValue,
            BannedBy = "tester",
            BanType = BanType.Server,
            Reason = "what a great test",
            Source = sourceB.Id,
            SourceNavigation = sourceB
        };

        var comparer = BanEqualityComparer.Instance;
        Assert.False(comparer.Equals(banA, banB), "Two bans from different sources should not be equal by internal values");
        Assert.False(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Two bans from different sources should not have equal hashcodes");
    }

    [Fact]
    public void Equals_SameBanByBanID_ReturnTrue()
    {
        var banA = new Ban
        {
            BanID = "epic",
            CKey = "doesn't matter"
        };

        var banB = new Ban
        {
            BanID = "epic",
            CKey = "different"
        };

        var comparer = BanEqualityComparer.Instance;
        Assert.True(comparer.Equals(banA, banB), "Two bans with BanIDs should be checked for equality by ID");
        Assert.True(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Two bans with BanIDs that are equal should have equal hashcodes");
    }

    [Fact]
    public void Equals_SameBanIDDifferentSource_ReturnFalse()
    {
        var sourceA = new BanSource
        {
            Display = "test source A",
            Name = "testA",
            RoleplayLevel = RoleplayLevel.Medium,
            Id = 3
        };

        var sourceB = new BanSource
        {
            Display = "test source B",
            Name = "testB",
            RoleplayLevel = RoleplayLevel.Medium,
            Id = 4
        };

        var banA = new Ban
        {
            BanID = "epic",
            CKey = "doesn't matter",
            Source = sourceA.Id,
            SourceNavigation = sourceA
        };

        var banB = new Ban
        {
            BanID = "epic",
            CKey = "different",
            Source = sourceB.Id,
            SourceNavigation = sourceB
        };

        var comparer = BanEqualityComparer.Instance;
        Assert.False(comparer.Equals(banA, banB), "Two bans from different sources should not be equal by BanID");
        Assert.False(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Two bans from different sources should not have equal hashcodes");
    }

    [Fact]
    public void Equals_SameBanDifferentJobOrder_ReturnTrue()
    {
        var banA = new Ban
        {
            Id = 12,
            BanType = BanType.Job
        };
        banA.AddJobRange(new[] { "detective", "head of security", "security officer", "warden" });

        var banB = new Ban
        {
            Id = 0,
            BanType = BanType.Job
        };
        banB.AddJobRange(new[] { "head of security", "warden", "detective", "security officer" });

        var comparer = BanEqualityComparer.Instance;
        Assert.True(comparer.Equals(banA, banB), "Two bans with the same jobbans in different orders should be equal");
        Assert.True(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Two bans with the same jobbans in different orders should be equal");
    }

    [Fact]
    public void Equals_SameBanNullVsEmptyJobBans_ReturnTrue()
    {
        var banA = new Ban
        {
            Id = 0,
            Source = 15,
            BanType = BanType.Server,
            JobBans = null
        };

        var banB = new Ban
        {
            Id = 0,
            Source = 15,
            BanType = BanType.Server
        };

        var comparer = BanEqualityComparer.Instance;
        Assert.True(comparer.Equals(banA, banB), "Bans should be equal if the jobbans only differ by null and an empty set");
        Assert.True(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Bans should have the same hashcode if the jobbans only differ by null and an empty set");
    }

    [Fact]
    public void Equals_SameBanDifferingAttributes_ReturnFalse()
    {
        var banA = new Ban
        {
            Id = 0,
            Source = 15
        };

        var banB = new Ban
        {
            Id = 0,
            Source = 15
        };
        banB.AddAttribute(BanAttribute.BeeStationGlobal);

        var comparer = BanEqualityComparer.Instance;
        Assert.False(comparer.Equals(banA, banB), "Bans should not be equal if they differ in attributes");
        Assert.False(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Bans should not have the same hashcode if they differ in attributes");
    }

    [Fact]
    public void Equals_SameBanSameAttributes_ReturnTrue()
    {
        var banA = new Ban
        {
            Id = 0,
            Source = 15
        };
        banA.AddAttribute(BanAttribute.BeeStationGlobal);

        var banB = new Ban
        {
            Id = 0,
            Source = 15
        };
        banB.AddAttribute(BanAttribute.BeeStationGlobal);

        var comparer = BanEqualityComparer.Instance;
        Assert.True(comparer.Equals(banA, banB), "Bans should be equal when they are equal including attributes");
        Assert.True(comparer.GetHashCode(banA) == comparer.GetHashCode(banB), "Bans should have the same hashcode if they are equal including attributes");
    }
}