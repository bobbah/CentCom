using System;
using System.Text.Json.Serialization;
using CentCom.Common.Extensions;
using CentCom.Exporter.Configuration;
using CentCom.Exporter.Data.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("hostsettings.json", optional: true)
    .AddJsonFile($"hostsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Insert(0, new JsonStringEnumConverter());
        options.JsonSerializerOptions.AddCentComOptions();
    });

builder.Services.AddOptions<BanProviderOptions>().Bind(builder.Configuration.GetSection("CentCom"));

if (!Enum.TryParse<BanProviderKind>(builder.Configuration["CentCom:Provider"], out var providerKind))
    throw new InvalidOperationException("Invalid or unknown ban provider kind found in configuration");

switch (providerKind)
{
    case BanProviderKind.Tgstation:
        builder.Services.AddTransient<IBanProvider, TgBanProvider>();
        break;
    case BanProviderKind.ParadiseSS13:
        builder.Services.AddTransient<IBanProvider, ParadiseBanProvider>();
        break;
    default:
        throw new InvalidOperationException($"Unsupported ban provider kind: {providerKind}");
}

var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
