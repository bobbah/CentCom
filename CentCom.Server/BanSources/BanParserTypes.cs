using System;

namespace CentCom.Server.BanSources;

internal static class BanParserTypes
{
    internal static readonly Type[] All =
    [
        typeof(BeeBanParser),
        typeof(FulpBanParser),
        typeof(StandardBanParser),
        typeof(TgBanParser),
        typeof(TGMCBanParser),
        typeof(VgBanParser),
        typeof(YogBanParser)
    ];
}
