using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CentCom.Server.External;
using CentCom.Server.External.Raw;
using CentCom.Server.Services;
using Xunit;

namespace CentCom.Test.BanServices;

public class TgBanServiceTests
{
    [Fact]
    public async Task TgBans_ShouldParseData()
    {
        var testData = await File.ReadAllTextAsync("BanServices/TgBanSample.json");
        var result = JsonSerializer.Deserialize<TgApiResponse>(testData, TgBanService.JsonOptions);
        var banData = result.Data.Select(x => x.AsBan(null));
        Assert.NotNull(result);
        Assert.NotEmpty(banData);
    }
}