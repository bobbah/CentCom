using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using CentCom.API.Services;
using CentCom.API.Services.Implemented;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CentCom
{
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
            services.AddControllers().AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                x.JsonSerializerOptions.IgnoreNullValues = true;
            });

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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CentCom",
                    Version = "v1",
                    Description = "An API for accesing CentCom, a central ban intelligence service for Space Station 13 servers"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CentCom V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
