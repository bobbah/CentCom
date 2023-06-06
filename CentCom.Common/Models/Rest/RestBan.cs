using System;
using System.Collections.Generic;
using CentCom.Common.Abstract;
using Remora.Rest.Core;

namespace CentCom.Common.Models.Rest;

public record RestBan
(
    int Id,
    BanType BanType,
    ICKey CKey,
    DateTimeOffset BannedOn,
    ICKey BannedBy,
    string Reason,
    DateTimeOffset? Expires,
    ICKey UnbannedBy,
    IReadOnlyList<IRestJobBan> JobBans,
    Optional<int?> RoundId
) : IRestBan;