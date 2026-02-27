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
    }

    private static JsonSerializerOptions GetOptions() =>
        new ServiceCollection().AddCentComSerialization().BuildServiceProvider()
        .GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
}