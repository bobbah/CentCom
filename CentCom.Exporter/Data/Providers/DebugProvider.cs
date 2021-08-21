using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Common.Models;
using CentCom.Common.Models.Byond;
using CentCom.Common.Models.Rest;
using CentCom.Exporter.Configuration;

namespace CentCom.Exporter.Data.Providers
{
    public class DebugProvider : IBanProvider
    {
        public async Task<IEnumerable<IRestBan>> GetBansAsync(int? cursor, BanProviderOptions options)
        {
            IRestBan ban = new RestBan(
                1,
                BanType.Job,
                new CKey("Bobbahbrown"),
                DateTimeOffset.Now,
                new CKey("Pomf"),
                "Test ban please ignore",
                null,
                null,
                new[] { new RestJobBan("Janitor") });

            return new[] { ban };
        }
    }
}