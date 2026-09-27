using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.API.Services.Implemented;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace CentCom.API;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddJsonOptions(x => ConfigureJsonOptions(x.JsonSerializerOptions))
            .AddRazorRuntimeCompilation();
        services.ConfigureHttpJsonOptions(x => ConfigureJsonOptions(x.SerializerOptions));

        // Add DB context
        var dbConfig = new DbConfig();
        Configuration.Bind("dbConfig", dbConfig);
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

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "CentCom";
                document.Info.Version = statusService.GetVersion().ToString();
                document.Info.Description = "An API for accessing CentCom, a central ban intelligence service for Space Station 13 servers";
                return Task.CompletedTask;
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapOpenApi();
            endpoints.MapOpenApi("/swagger/{documentName}/swagger.json");
            endpoints.MapScalarApiReference("/scalar", options => options
                .WithTitle("CentCom API Documentation")
                .HideClientButton()
                .HideDeveloperTools()
                .DisableAgent()
                .DisableMcp()
                .WithCustomCss("a[href=\"https://www.scalar.com\"] { display: none; }"));
            endpoints.MapGet("/swagger", () => Results.Redirect("/scalar", permanent: true))
                .ExcludeFromDescription();
            endpoints.MapGet("/swagger/index.html", () => Results.Redirect("/scalar", permanent: true))
                .ExcludeFromDescription();
            endpoints.MapControllerRoute("default", "{controller=Viewer}/{action=Index}/{id?}");
        });
    }

    private static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}