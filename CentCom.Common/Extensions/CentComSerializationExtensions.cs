using System.Text.Json;
using CentCom.Common.Abstract;
using CentCom.Common.Json;
using CentCom.Common.Models.Rest;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Extensions;

namespace CentCom.Common.Extensions
{
    public static class CentComSerializationExtensions
    {
        public static IServiceCollection AddCentComSerialization(this IServiceCollection services)
            => services.Configure<JsonSerializerOptions>(options =>
            {
                options.AddCentComOptions();
                options.AddDataObjectConverter<ICKey, RestCKey>();
                options.AddDataObjectConverter<IRestJobBan, RestJobBan>();
                options.AddDataObjectConverter<IRestBan, RestBan>();
            });

        public static JsonSerializerOptions AddCentComOptions(this JsonSerializerOptions options)
        {
            options
                .AddConverter<CKeyStringConverter>()
                .AddConverter<JobBanCollectionConverter>();

            return options;
        }
    }
}