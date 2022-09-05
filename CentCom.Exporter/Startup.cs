using System;
using System.Text.Json.Serialization;
using CentCom.Common.Extensions;
using CentCom.Exporter.Configuration;
using CentCom.Exporter.Data.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CentCom.Exporter;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Insert(0, new JsonStringEnumConverter());
                options.JsonSerializerOptions.AddCentComOptions();
            });

        // Add provider configuration
        services.AddOptions<BanProviderOptions>().Bind(Configuration.GetSection("CentCom"));

        // Add providers
        if (!Enum.TryParse(typeof(BanProviderKind), Configuration.GetSection("centcom")["provider"],
                out var providerKind))
            throw new Exception("Invalid or unknown ban provider kind found in configuration");
        switch (providerKind)
        {
            case BanProviderKind.Tgstation:
                services.AddTransient<IBanProvider, TgBanProvider>();
                break;
            case BanProviderKind.ParadiseSS13:
                services.AddTransient<IBanProvider, ParadiseBanProvider>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}