using CentCom.Common.Models;

namespace CentCom.Server.Configuration
{
    public class StandardProviderConfiguration
    {
        public string Id { get; set; }
        public string Display { get; set; }
        public RoleplayLevel RoleplayLevel { get; set; }
        public string Url { get; set; }
    }
}