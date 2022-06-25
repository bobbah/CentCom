using CentCom.Common.Models;

namespace CentCom.Server.External.Raw;

interface IRawBan
{
    BanType GetBanType();
    Ban AsBan(BanSource source);
}