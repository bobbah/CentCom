using System.Collections.Generic;
using CentCom.Common.Models;

namespace CentCom.API.Models;

public record BanSearchPage(IReadOnlyList<KeySummary> Data, int Page, bool HasNextPage);
