using System.Net.Http;
using System.Threading.Tasks;
using CentCom.Server.Services;
using Xunit;

namespace CentCom.Test.BanServices;

public class BeeBanServiceTests
{
    [Fact]
    public async Task BeeBans_ShouldGetPages()
    {
        var toTest = new BeeBanService(new HttpClient(), null);
        var result = await toTest.GetNumberOfPagesAsync();
        Assert.NotEqual(0, result);
    }

    [Fact]
    public async Task BeeBans_ShouldGetBans()
    {
        var toTest = new BeeBanService(new HttpClient(), null);
        var result = await toTest.GetBansAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}