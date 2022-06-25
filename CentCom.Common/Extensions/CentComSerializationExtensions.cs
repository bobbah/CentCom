using System.Text.Json;
using CentCom.Common.Abstract;
using CentCom.Common.Json;
using CentCom.Common.Models.Rest;
using Microsoft.Extensions.DependencyInjection;
using Remora.Rest.Extensions;

namespace CentCom.Common.Extensions;

/// <summary>
/// Extension methods used to add JSON serialization options to support serialization of ICKey, IRestBan, and
/// IRestJobBan objects
/// </summary>
public static class CentComSerializationExtensions
{
    public static IServiceCollection AddCentComSerialization(this IServiceCollection services)
        => services.Configure<JsonSerializerOptions>(options =>
        {
            options.AddCentComOptions();
            options.AddDataObjectConverter<IRestBan, RestBan>();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

    public static JsonSerializerOptions AddCentComOptions(this JsonSerializerOptions options)
    {
        options
            .AddConverter<CKeyStringConverter>()
            .AddConverter<JobBanCollectionConverter>();

        return options;
    }
}