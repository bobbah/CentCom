using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.API.Services.Implemented;
using CentCom.Common.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("hostsettings.json", optional: true)
    .AddJsonFile($"hostsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var mvc = builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => ConfigureJsonOptions(options.JsonSerializerOptions));
if (builder.Environment.IsDevelopment())
    mvc.AddRazorRuntimeCompilation();
builder.Services.ConfigureHttpJsonOptions(options => ConfigureJsonOptions(options.SerializerOptions));

builder.Services.AddCentComDatabase(builder.Configuration);

builder.Services.AddTransient<IBanService, BanService>();
builder.Services.AddTransient<IBanSourceService, BanSourceService>();
builder.Services.AddSingleton<IAppStatusService, AppStatusService>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info.Title = "CentCom";
        document.Info.Version = context.ApplicationServices.GetRequiredService<IAppStatusService>().GetVersion().ToString();
        document.Info.Description = "An API for accessing CentCom, a central ban intelligence service for Space Station 13 servers";
        document.Servers = [new OpenApiServer { Url = "/" }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapOpenApi();
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/scalar", options => options
    .WithTitle("CentCom API Documentation")
    .HideClientButton()
    .HideDeveloperTools()
    .DisableAgent()
    .DisableMcp()
    .WithCustomCss("a[href=\"https://www.scalar.com\"] { display: none; }"));
app.MapGet("/swagger", () => Results.Redirect("/scalar", permanent: true))
    .ExcludeFromDescription();
app.MapGet("/swagger/index.html", () => Results.Redirect("/scalar", permanent: true))
    .ExcludeFromDescription();
app.MapControllerRoute("default", "{controller=Viewer}/{action=Index}/{id?}");

app.Run();

static void ConfigureJsonOptions(JsonSerializerOptions options)
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
}
