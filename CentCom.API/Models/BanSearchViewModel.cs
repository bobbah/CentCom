using CentCom.Common.Models;
using System.Collections.Generic;

namespace CentCom.API.Models
{
    public class BanSearchViewModel
    {
        public string CKey { get; set; }
        public IEnumerable<KeySummary> Data { get; set; }
    }
}
