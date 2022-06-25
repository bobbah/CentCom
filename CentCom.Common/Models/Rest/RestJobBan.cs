using CentCom.Common.Abstract;

namespace CentCom.Common.Models.Rest;

public record RestJobBan(string Job) : IRestJobBan
{
    public static implicit operator RestJobBan(string job) => new RestJobBan(job);
}