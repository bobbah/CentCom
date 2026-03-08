using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.API.Models;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace CentCom.API.Services.Implemented;

public class BanSourceService : IBanSourceService
{
    private readonly DatabaseContext _dbContext;

    public BanSourceService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync()
    {
        return await _dbContext.BanSources.Select(x => BanSourceData.FromBanSource(x)).ToListAsync();
    }

    public async Task<BanSourceData> GetBanSourceAsync(int source)
    {
        return BanSourceData.FromBanSource(await _dbContext.BanSources.FirstOrDefaultAsync(x => x.Id == source));
    }
}