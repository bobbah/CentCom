using System.Collections.Generic;
using CentCom.Common.Models;

namespace CentCom.API.Models;

public class BanSearchViewModel
{
    public string CKey { get; set; }
    public IEnumerable<KeySummary> Data { get; set; }
}