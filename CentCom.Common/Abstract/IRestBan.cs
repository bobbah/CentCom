using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Common.Models;

namespace CentCom.Common.Abstract
{
    public interface IRestBan
    {
        public int Id { get; }
        public BanType BanType { get; }
        public ICKey CKey { get; }
        public DateTimeOffset BannedOn { get; }
        public ICKey BannedBy { get; }
        public string Reason { get; }
        public DateTimeOffset? Expires { get; }
        public ICKey UnbannedBy { get; }
        public IReadOnlyList<IRestJobBan> JobBans { get; }
    }
}