using CentCom.Common.Models;

namespace CentCom.API.Models
{
    public class BanSourceData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public RoleplayLevel RoleplayLevel { get; set; }

        public static BanSourceData FromBanSource(BanSource source)
        {
            return new BanSourceData()
            {
                ID = source.Id,
                Name = source.Display,
                RoleplayLevel = source.RoleplayLevel
            };
        }
    }
}
