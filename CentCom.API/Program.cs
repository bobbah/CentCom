using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using CentCom.API.Services;
using CentCom.API.Services.Implemented;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CentCom.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional: true)
            .AddCommandLine(args)
            .Build();
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureApplication(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddRouting(o => o.LowercaseUrls = true);

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }).AddRazorRuntimeCompilation();
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc(options => { options.Conventions.Add(new VersionByNamespaceConvention()); })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.ApiVersionParameterSource = new UrlSegmentApiVersionReader();
                options.SubstituteApiVersionInUrl = true;
            });
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

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
                services.AddDbContext<DatabaseContext, NpgsqlDbContext>();
                break;
            case DbType.MariaDB:
            case DbType.MySql:
                services.AddDbContext<DatabaseContext, MySqlDbContext>();
                break;
        }

        services.AddTransient<IBanService, BanService>();
        services.AddTransient<IBanSourceService, BanSourceService>();

        // Add status service
        var statusService = new AppStatusService();
        services.AddSingleton<IAppStatusService>(statusService);
    }

    private static void ConfigureApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            var latest = true;
            foreach (var desc in app.DescribeApiVersions().Reverse())
            {
                var url = $"/swagger/{desc.GroupName}/swagger.json";
                var name = $"{desc.GroupName.ToUpperInvariant()}{(latest ? " (Latest)" : null)}";
                c.SwaggerEndpoint(url, name);
                latest = false;
            }
        });

        app.UseRouting();

        app.UseCors(o => o.AllowAnyOrigin());

        app.UseAuthorization();

        app.MapControllers();
    }
}