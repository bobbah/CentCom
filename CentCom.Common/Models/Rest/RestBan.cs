using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Common.Abstract;

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
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? RoundId
) : IRestBan;