using System.Net;
using System.Net.Http;
using PDFCreatorUpdater.Exception;

namespace PDFCreatorUpdater.Extensions;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> Get(this HttpClient client, string url)
    {
        var message = await client.GetAsync(url);

        return message.StatusCode switch
        {
            HttpStatusCode.OK => message,
            _ => throw new GitHubRequestFailedException(message)
        };
    }
}