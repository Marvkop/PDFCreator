using System.IO;
using System.IO.Compression;
using System.Net.Http;
using PDFCreatorUpdater.Data;
using PDFCreatorUpdater.Extensions;
using PDFCreatorUpdater.Helper;

namespace PDFCreatorUpdater.Service;

public class GitHubService
{
    private readonly HttpClient _client = new();

    public GitHubService()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", "PDFCreator by Marvkop");
    }

    public async Task<string[]> Download(string assetUrl)
    {
        var result = await _client.Get(assetUrl);
        var assetResponses = result.GetContentAs<GitHubAssetResponse[]>();
        var files = new List<string>();

        foreach (var assetResponse in assetResponses)
        {
            var extension = Path.GetExtension(assetResponse.FileName);

            switch (extension)
            {
                case ".zip":
                {
                    files.AddRange(await DownloadZip(assetResponse.DownloadUrl));
                    break;
                }

                default:
                {
                    files.Add(await Download(assetResponse.DownloadUrl, assetResponse.FileName));
                    break;
                }
            }
        }

        return files.ToArray();
    }

    private async Task<string[]> DownloadZip(string downloadUrl)
    {
        var result = await _client.Get(downloadUrl);
        await using var stream = await result.Content.ReadAsStreamAsync();
        var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var files = new List<string>();

        foreach (var entry in archive.Entries)
        {
            var open = entry.Open();
            var file = FileService.SaveToFile(open, entry.Name);
            files.Add(file);
        }

        return files.ToArray();
    }

    private async Task<string> Download(string downloadUrl, string name)
    {
        var result = await _client.Get(downloadUrl);
        await using var stream = await result.Content.ReadAsStreamAsync();

        return FileService.SaveToFile(stream, name);
    }

    public async Task<GitHubLatestReleaseResponse> GetLatestRelease(string owner, string repo)
    {
        var response = await _client.Get($"{GitHubHelper.GetBaseUri(owner, repo)}/releases/latest");
        response.EnsureSuccessStatusCode();
        return response.GetContentAs<GitHubLatestReleaseResponse>();
    }
}