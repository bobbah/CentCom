using CentCom.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CentCom.Server.FlatData
{
    public interface IFlatDataSource
    {
        public string SourceDisplayName();
        public IEnumerable<Ban> GetBans();
        public IEnumerable<BanSource> GetSources();
    }
}
