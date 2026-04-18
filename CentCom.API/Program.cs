using System.Text.Json.Serialization;
using MudBlazor.Services;
using CentCom.API.Components;
using CentCom.API.Services;
using CentCom.API.Services.Implemented;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add controllers for API
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
        builder.Services.AddDbContextFactory<NpgsqlDbContext>();
        break;
    case DbType.MariaDB:
    case DbType.MySql:
        throw new Exception(
            "MySQL/MariaDB support is not currently supported for the API server, please use Postgres for now");
}

builder.Services.AddTransient<IBanService, BanService>();
builder.Services.AddTransient<IBanSourceService, BanSourceService>();
builder.Services.AddBlazorLocalTimeService();

// Add status service
var statusService = new AppStatusService();
builder.Services.AddSingleton<IAppStatusService>(statusService);

// Add OpenAPI
builder.Services.AddOpenApi(o =>
{
    o.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "CentCom";
        document.Info.Version = statusService.GetVersion().ToString();

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapOpenApi();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/openapi/v1.json", $"CentCom {statusService.GetVersion().ToString()}");
    o.RoutePrefix = "swagger";
});

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();