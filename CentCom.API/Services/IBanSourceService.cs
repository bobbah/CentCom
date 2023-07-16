using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.V1.Models;

namespace CentCom.API.Services;

public interface IBanSourceService
{
    public Task<BanSourceData> GetBanSourceAsync(int source);
    public Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync();
}