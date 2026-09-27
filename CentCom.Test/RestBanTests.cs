using System.Text.Json;
using CentCom.Common.Abstract;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Byond;
using CentCom.Common.Models.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CentCom.Test;

public class RestBanTests
{
    [Test]
    public async Task CanCreateBan()
    {
        IRestBan ban = new RestBan(
            1,
            BanType.Job,
            new CKey("Bobbahbrown"),
            DateTimeOffset.Now,
            new CKey("Pomf"),
            "Test ban please ignore",
            null,
            null,
            new[] { new RestJobBan("Janitor") },
            null);
        await Assert.That(ban).IsNotNull();
    }

    [Test]
    public async Task CanSerializeBan()
    {
        IRestBan ban = new RestBan(
            1,
            BanType.Job,
            new CKey("Bobbahbrown"),
            DateTimeOffset.Now,
            new CKey("Pomf"),
            "Test ban please ignore",
            null,
            null,
            [new RestJobBan("Janitor")],
            null);

        var options = GetOptions();
        var serialized = JsonSerializer.Serialize(ban, options);
        var deserialized = JsonSerializer.Deserialize<IRestBan>(serialized, options);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.JobBans.Count).IsEqualTo(1);
        await Assert.That(deserialized.JobBans[0].Job).IsEqualTo("Janitor");
    }

    [Test]
    public async Task JobBansRoundTripWithoutReadingPastArray()
    {
        var options = GetOptions();
        var ban = new RestBan(
            1, BanType.Job, new CKey("Bobbahbrown"), DateTimeOffset.UtcNow,
            new CKey("Pomf"), "Test ban", null, null,
            [new RestJobBan("Janitor"), new RestJobBan(""), new RestJobBan("Engineer")], 42);

        var serialized = JsonSerializer.Serialize(ban, options);
        var deserialized = JsonSerializer.Deserialize<IRestBan>(serialized, options);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.JobBans.Count).IsEqualTo(3);
        await Assert.That(deserialized.JobBans[0].Job).IsEqualTo("Janitor");
        await Assert.That(deserialized.JobBans[1].Job).IsEqualTo("");
        await Assert.That(deserialized.JobBans[2].Job).IsEqualTo("Engineer");
        await Assert.That(deserialized.RoundId).IsEqualTo(42);
    }

    [Test]
    public async Task JobBanCollectionPreservesEmptyAndNullCollections()
    {
        var options = GetOptions();
        IReadOnlyList<IRestJobBan> empty = [];

        await Assert.That(JsonSerializer.Serialize(empty, options)).IsEqualTo("[]");
        await Assert.That(JsonSerializer.Deserialize<IReadOnlyList<IRestJobBan>>("[]", options)!.Count)
            .IsEqualTo(0);
        await Assert.That(JsonSerializer.Serialize<IReadOnlyList<IRestJobBan>>(null!, options)).IsEqualTo("null");
        await Assert.That(JsonSerializer.Deserialize<IReadOnlyList<IRestJobBan>>("null", options)).IsNull();
    }

    [Test]
    public async Task JobBanCollectionRejectsNullEntries()
    {
        var options = GetOptions();
        var rejectedRead = false;
        try
        {
            JsonSerializer.Deserialize<IReadOnlyList<IRestJobBan>>("[\"Janitor\",null]", options);
        }
        catch (JsonException)
        {
            rejectedRead = true;
        }

        await Assert.That(rejectedRead).IsTrue();

        foreach (IReadOnlyList<IRestJobBan> jobs in new IReadOnlyList<IRestJobBan>[]
                 {
                     [new RestJobBan("Janitor"), null!],
                     [new RestJobBan(null!)]
                 })
        {
            var rejectedWrite = false;
            try
            {
                JsonSerializer.Serialize(jobs, options);
            }
            catch (JsonException)
            {
                rejectedWrite = true;
            }

            await Assert.That(rejectedWrite).IsTrue();
        }
    }

    [Test]
    public async Task JobBanCollectionRejectsUnexpectedTokens()
    {
        var options = GetOptions();
        foreach (var json in new[] { "\"Janitor\"", "{}", "[1]", "[true]", "[{}]", "[[\"Janitor\"]]", "[\"Janitor\"" })
        {
            var rejected = false;
            try
            {
                JsonSerializer.Deserialize<IReadOnlyList<IRestJobBan>>(json, options);
            }
            catch (JsonException)
            {
                rejected = true;
            }

            await Assert.That(rejected).IsTrue();
        }
    }

    private static JsonSerializerOptions GetOptions() =>
        new ServiceCollection().AddCentComSerialization().BuildServiceProvider()
        .GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
}