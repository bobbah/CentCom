using System.Collections.Generic;
using CentCom.Common.Models.DTO;

namespace CentCom.API.Models;

public class BanViewViewModel
{
    public string CKey { get; set; }
    public bool OnlyActive { get; set; }
    public IEnumerable<BanData> Bans { get; set; }
}