using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Exporter.Configuration;

namespace CentCom.Exporter.Data.Providers
{
    public interface IBanProvider
    {
        public Task<IEnumerable<IRestBan>> GetBansAsync(int? cursor, BanProviderOptions options);
    }
}