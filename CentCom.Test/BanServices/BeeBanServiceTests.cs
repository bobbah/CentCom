using CentCom.Server.Services;

namespace CentCom.Test.BanServices;

public class BeeBanServiceTests
{
    [Test]
    public async Task BeeBans_ShouldGetPages()
    {
        var toTest = new BeeBanService(new HttpClient(), null);
        var result = await toTest.GetNumberOfPagesAsync();
        await Assert.That(result).IsNotEqualTo(0);
    }

    [Test]
    public async Task BeeBans_ShouldGetBans()
    {
        var toTest = new BeeBanService(new HttpClient(), null);
        var result = await toTest.GetBansAsync();
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsNotEmpty();
    }
}