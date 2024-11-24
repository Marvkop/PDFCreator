using Newtonsoft.Json;

namespace PDFCreatorUpdater.Data;

public record GitHubLatestReleaseResponse(
    [JsonProperty("assets_url")] string AssetsUrl,
    [JsonProperty("name")] string Name,
    [JsonProperty("created_at")] string Created
);