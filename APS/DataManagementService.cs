using APS_Forma_Console.Auth;
using APS_Forma_Console.Utils;
using System.Text.Json;

namespace APS_Forma_Console.APS;
internal class DataManagementService(AuthService authService)
{
    public Task<JsonDocument> GetHubs() =>
        authService.GetJson(Endpoints.Hubs());

    public Task<JsonDocument> GetProjects(string hubId) =>
        authService.GetJson(Endpoints.Projects(hubId));

    public Task<JsonDocument> GetTopFolders(string hubId, string projectId) =>
        authService.GetJson(Endpoints.TopFolders(hubId, projectId));

    public Task<JsonDocument> GetFolderContents(string projectId, string folderId) =>
        authService.GetJson(Endpoints.FolderContents(projectId, folderId));

    public Task<JsonDocument> GetItemVersions(string projectId, string itemId) =>
        authService.GetJson(Endpoints.ItemVersions(projectId, itemId));

    public async Task<JsonElement> GetLatestVersion(string projectId, string itemId)
    {
        using JsonDocument versions = await GetItemVersions(projectId, itemId);
        JsonElement data = versions.RootElement.GetProperty("data");

        if (data.GetArrayLength() == 0)
            throw new InvalidOperationException("Selected item has no versions.");

        return data[0].Clone();
    }

}
