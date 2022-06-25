namespace CentCom.Common.Configuration;

public enum DbType
{
    Postgres,
    MySql,
    MariaDB
}

/// <summary>
/// Configuration for a database to be used
/// </summary>
public class DbConfig
{
    /// <summary>
    /// The SQL connection string for the database
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// The type of database, used for initializing data contexts
    /// </summary>
    public DbType DbType { get; set; }
}