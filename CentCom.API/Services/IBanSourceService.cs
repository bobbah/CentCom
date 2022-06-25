using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Models;

namespace CentCom.API.Services;

public interface IBanSourceService
{
    public Task<BanSourceData> GetBanSourceAsync(int source);
    public Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync();
}