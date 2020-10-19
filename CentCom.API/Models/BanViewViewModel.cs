using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentCom.API.Models
{
    public class BanViewViewModel
    {
        public string CKey { get; set; }
        public bool OnlyActive { get; set; }
        public IEnumerable<BanData> Bans { get; set; }
    }
}
