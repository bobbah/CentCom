using CentCom.API.Models;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace CentCom.API.Services.Implemented;

public class BanSourceService(IDbContextFactory<NpgsqlDbContext> dbContextFactory) : IBanSourceService
{
    public async Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BanSources.Select(x => BanSourceData.FromBanSource(x)).ToListAsync();
    }

    public async Task<BanSourceData> GetBanSourceAsync(int source)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        return BanSourceData.FromBanSource(await dbContext.BanSources.FirstOrDefaultAsync(x => x.Id == source));
    }
}