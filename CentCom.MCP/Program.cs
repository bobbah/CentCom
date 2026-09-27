using CentCom.Common.Configuration;
using CentCom.MCP.Services;
using CentCom.MCP.Services.Implemented;
using CentCom.MCP.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("hostsettings.json", optional: true)
    .AddJsonFile($"hostsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddCentComDatabase(builder.Configuration);

builder.Services.AddTransient<IBanDataService, BanDataService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
app.MapMcp();

app.Run();