using CentCom.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentCom.API.Models
{
    public class BanSearchViewModel
    {
        public string CKey { get; set; }
        public IEnumerable<KeySummary> Data { get; set; }
    }
}
