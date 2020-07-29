using CentCom.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentCom.API.Services
{
    public interface IBanSourceService
    {
        public Task<BanSourceData> GetBanSourceAsync(int source);
        public Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync();
    }
}
