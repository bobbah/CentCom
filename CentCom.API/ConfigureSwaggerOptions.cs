using System;
using System.IO;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using CentCom.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CentCom.API;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IAppStatusService _status;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IAppStatusService status)
    {
        _provider = provider;
        _status = status;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.OperationFilter<DeprecatedFilter>();

        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo()
            {
                Title = $"CentCom API {_status.GetVersion()}",
                Version = $"API v{desc.ApiVersion}",
                Description = desc.IsDeprecated
                    ? "Note: This version of the API is obsolete, you should use the latest version of the API."
                    : null
            });
        }

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    }
}