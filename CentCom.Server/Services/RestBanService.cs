using System.Reflection;
using System.Text;
using CentCom.Server.Exceptions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CentCom.Server.Services;

public abstract class RestBanService
{
    private readonly ILogger _logger;

    protected RestBanService(ILogger logger)
    {
        _logger = logger;
        InitializeClient();
    }

    protected RestClient Client { get; private set; }
    protected abstract string BaseUrl { get; }

    protected void FailedRequest(RestResponse response)
    {
        // Build error
        var url = Client.BuildUri(response.Request);
        var messageBuilder =
            new StringBuilder(
                $"Source website returned a non-200 HTTP response code.\n\tCode: {response.StatusCode}");
        if (url != response.ResponseUri) // Add redirected URL if present
            messageBuilder.Append($"\n\tResponse URL: \"{response.ResponseUri}\"");
        messageBuilder.Append($"\n\tRequest URL: \"{url}\"");
        var message = messageBuilder.ToString();
            
        // Log error as appropriate
        _logger.LogError(message);
        throw new BanSourceUnavailableException(message, response.Content);
    }


    protected void InitializeClient()
    {
        if (BaseUrl == null)
            return;
            
        var options = new RestClientOptions(BaseUrl)
        {
            // Setup user agent
            UserAgent =
                $"Mozilla/5.0 (compatible; CentComBot/{Assembly.GetExecutingAssembly().GetName().Version}; +https://centcom.melonmesa.com/scraper)"
        };

        Client = new RestClient(options);
    }
}