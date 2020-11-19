namespace CentCom.Common.Configuration
{
    public enum DbType
    {
        Postgres,
        MySql,
        MariaDB
    }

    public class DbConfig
    {
        public string ConnectionString { get; set; }
        public DbType DbType { get; set; }
    }
}
