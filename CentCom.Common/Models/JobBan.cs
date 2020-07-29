namespace CentCom.Common.Models
{
    public class JobBan
    {
        public int BanId { get; set; }
        public virtual Ban BanNavigation { get; set; }
        public string Job { get; set; }
    }
}
