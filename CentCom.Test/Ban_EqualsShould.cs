using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Equality;

namespace CentCom.Test;

public class Ban_EqualsShould
{
    [Test]
    public async Task Equals_SameBanDifferentID_ReturnTrue()
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
        await Assert.That(comparer.Equals(banA, banB)).IsTrue().Because("Two bans equal by internal values should be equal");
        await Assert.That(comparer.GetHashCode(banA)).IsEqualTo(comparer.GetHashCode(banB)).Because("Two bans equal by internal values should have equal hashcodes");
    }

    [Test]
    public async Task Equals_SameBanDifferentIDDifferentSource_ReturnFalse()
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
        await Assert.That(comparer.Equals(banA, banB)).IsFalse().Because("Two bans from different sources should not be equal by internal values");
        await Assert.That(comparer.GetHashCode(banA)).IsNotEqualTo(comparer.GetHashCode(banB)).Because("Two bans from different sources should not have equal hashcodes");
    }

    [Test]
    public async Task Equals_SameBanByBanID_ReturnTrue()
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
        await Assert.That(comparer.Equals(banA, banB)).IsTrue().Because("Two bans with BanIDs should be checked for equality by ID");
        await Assert.That(comparer.GetHashCode(banA)).IsEqualTo(comparer.GetHashCode(banB)).Because("Two bans with BanIDs that are equal should have equal hashcodes");
    }

    [Test]
    public async Task Equals_SameBanIDDifferentSource_ReturnFalse()
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
        await Assert.That(comparer.Equals(banA, banB)).IsFalse().Because("Two bans from different sources should not be equal by BanID");
        await Assert.That(comparer.GetHashCode(banA)).IsNotEqualTo(comparer.GetHashCode(banB)).Because("Two bans from different sources should not have equal hashcodes");
    }

    [Test]
    public async Task Equals_SameBanDifferentJobOrder_ReturnTrue()
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
        await Assert.That(comparer.Equals(banA, banB)).IsTrue().Because("Two bans with the same jobbans in different orders should be equal");
        await Assert.That(comparer.GetHashCode(banA)).IsEqualTo(comparer.GetHashCode(banB)).Because("Two bans with the same jobbans in different orders should be equal");
    }

    [Test]
    public async Task Equals_SameBanNullVsEmptyJobBans_ReturnTrue()
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
        await Assert.That(comparer.Equals(banA, banB)).IsTrue().Because("Bans should be equal if the jobbans only differ by null and an empty set");
        await Assert.That(comparer.GetHashCode(banA)).IsEqualTo(comparer.GetHashCode(banB)).Because("Bans should have the same hashcode if the jobbans only differ by null and an empty set");
    }

    [Test]
    public async Task Equals_SameBanDifferingAttributes_ReturnFalse()
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
        await Assert.That(comparer.Equals(banA, banB)).IsFalse().Because("Bans should not be equal if they differ in attributes");
        await Assert.That(comparer.GetHashCode(banA)).IsNotEqualTo(comparer.GetHashCode(banB)).Because("Bans should not have the same hashcode if they differ in attributes");
    }

    [Test]
    public async Task Equals_SameBanSameAttributes_ReturnTrue()
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
        await Assert.That(comparer.Equals(banA, banB)).IsTrue().Because("Bans should be equal when they are equal including attributes");
        await Assert.That(comparer.GetHashCode(banA)).IsEqualTo(comparer.GetHashCode(banB)).Because("Bans should have the same hashcode if they are equal including attributes");
    }
}