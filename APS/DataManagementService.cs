using APS_Forma_Console.Auth;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APS_Forma_Console.APS;
internal class DataManagementService(AuthService authService)
{
    public Task<JsonDocument> GetHubs() =>
        GetJson(Endpoints.Hubs());

    public Task<JsonDocument> GetProjects(string hubId) =>
        GetJson(Endpoints.Projects(hubId));

    public Task<JsonDocument> GetTopFolders(string hubId, string projectId) =>
        GetJson(Endpoints.TopFolders(hubId, projectId));

    public Task<JsonDocument> GetFolderContents(string projectId, string folderId) =>
        GetJson(Endpoints.FolderContents(projectId, folderId));

    public Task<JsonDocument> GetItemVersions(string projectId, string itemId) =>
        GetJson(Endpoints.ItemVersions(projectId, itemId));

    public async Task<JsonElement> GetLatestVersion(string projectId, string itemId)
    {
        using JsonDocument versions = await GetItemVersions(projectId, itemId);
        JsonElement data = versions.RootElement.GetProperty("data");

        if (data.GetArrayLength() == 0)
            throw new InvalidOperationException("Selected item has no versions.");

        return data[0].Clone();
    }

    private async Task<JsonDocument> GetJson(string url)
    {
        using HttpClient httpClient = new();
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.GetToken());

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"GET failed: {(int)response.StatusCode} {url}{Environment.NewLine}{body}");

        return JsonDocument.Parse(body);
    }
}
