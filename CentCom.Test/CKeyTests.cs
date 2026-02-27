using System.Text.Json;
using CentCom.Common.Abstract;
using CentCom.Common.Extensions;
using CentCom.Common.Models.Byond;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CentCom.Test;

public class CKeyTests
{
    [Test]
    public async Task CKeyShouldCreate()
    {
        ICKey ckey = new CKey("Bobbahbrown");
        await Assert.That(ckey.CanonicalKey).IsEqualTo("bobbahbrown");
    }

    [Test]
    public async Task CKeyShouldCreateFromStringImplicitly()
    {
        CKey ckey = "Bobbahbrown";
        await Assert.That(ckey.CanonicalKey).IsEqualTo("bobbahbrown");
    }

    [Test]
    public async Task CKeyShouldSerialize()
    {
        var options = GetOptions();
        ICKey ckey = new CKey("Bobbahbrown");
        var serialized = JsonSerializer.Serialize(ckey, options);
        var deserialized = JsonSerializer.Deserialize<ICKey>(serialized, options);
        await Assert.That(deserialized?.CanonicalKey).IsEqualTo("bobbahbrown");
    }

    private static JsonSerializerOptions GetOptions() =>
        (new ServiceCollection()).AddCentComSerialization().BuildServiceProvider()
        .GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
}