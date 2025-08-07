using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MS.API.Mini.Services;

public class GitHubService(HttpClient httpClient, IConfiguration config, ILogger<GitHubService> logger)
{
    public async Task<List<ReleaseInfo>> GetReleasesAsync(string owner, string repo, int limit = 10)
    {
        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases?per_page={limit}";
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var releases = JsonSerializer.Deserialize<List<ReleaseInfo>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return releases ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching releases for {Owner}/{Repo}", owner, repo);
            throw;
        }
    }

    public async Task<ReleaseInfo> GetLatestReleaseAsync(string owner, string repo)
    {
        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var response = await httpClient.GetAsync(url);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning("No releases found for {Owner}/{Repo}", owner, repo);
                return null; // or a default response
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var release = JsonSerializer.Deserialize<ReleaseInfo>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return release;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching latest release for {Owner}/{Repo}", owner, repo);
            throw;
        }
    }
}

public class ReleaseInfo
{
    public string Id { get; set; }
    public string TagName { get; set; }
    public string Name { get; set; }
    public bool Prerelease { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Body { get; set; }
    public List<ReleaseAsset> Assets { get; set; }
}

public class ReleaseAsset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public string BrowserDownloadUrl { get; set; }
}