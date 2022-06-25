using System.Text.Json;
using CentCom.Common.Abstract;
using CentCom.Common.Extensions;
using CentCom.Common.Models.Byond;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CentCom.Test;

public class CKeyTests
{
    [Fact]
    public void CKeyShouldCreate()
    {
        ICKey ckey = new CKey("Bobbahbrown");
        Assert.Equal("bobbahbrown", ckey.CanonicalKey);
    }

    [Fact]
    public void CKeyShouldCreateFromStringImplicitly()
    {
        CKey ckey = "Bobbahbrown";
        Assert.Equal("bobbahbrown", ckey.CanonicalKey);
    }

    [Fact]
    public void CKeyShouldSerialize()
    {
        var options = GetOptions();
        ICKey ckey = new CKey("Bobbahbrown");
        var serialized = JsonSerializer.Serialize(ckey, options);
        var deserialized = JsonSerializer.Deserialize<ICKey>(serialized, options);
        Assert.Equal("bobbahbrown", deserialized?.CanonicalKey);
    }

    private static JsonSerializerOptions GetOptions()
    {
        return (new ServiceCollection()).AddCentComSerialization().BuildServiceProvider()
            .GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
    }
}