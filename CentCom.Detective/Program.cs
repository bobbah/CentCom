using CentCom.Detective;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(5178));
builder.Services.AddSingleton<EndpointTests>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPost("/test", async (TestRequest request, EndpointTests tests, CancellationToken cancellationToken) =>
{
    if (!Enum.TryParse<SourceType>(request.Type, true, out var type) ||
        !Enum.IsDefined(type) ||
        !Uri.TryCreate(request.Address, UriKind.Absolute, out var address) ||
        address.Scheme is not ("http" or "https") ||
        !string.IsNullOrEmpty(address.UserInfo) ||
        !string.IsNullOrEmpty(address.Query) ||
        !string.IsNullOrEmpty(address.Fragment))
        return Results.BadRequest(new
            { error = "Select a source and enter an HTTP(S) base URL without credentials, query or fragment." });

    return Results.Ok(await tests.RunAsync(type, address, cancellationToken));
});
app.Run();