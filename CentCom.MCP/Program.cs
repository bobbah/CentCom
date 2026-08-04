using CentCom.Common.Configuration;
using CentCom.Common.Data;
using CentCom.MCP.Services;
using CentCom.MCP.Services.Implemented;
using CentCom.MCP.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddUserSecrets<Program>()
    .AddJsonFile("hostsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Add DB context
var dbConfig = new DbConfig();
builder.Configuration.Bind("dbConfig", dbConfig);
if (dbConfig == null)
{
    throw new Exception("Failed to read DB configuration, please ensure you provide one in appsettings.json");
}
switch (dbConfig.DbType)
{
    case DbType.Postgres:
        builder.Services.AddDbContext<DatabaseContext, NpgsqlDbContext>();
        break;
    case DbType.MariaDB:
    case DbType.MySql:
        builder.Services.AddDbContext<DatabaseContext, MySqlDbContext>();
        break;
}

builder.Services.AddTransient<IBanDataService, BanDataService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
app.MapMcp();

app.Run();