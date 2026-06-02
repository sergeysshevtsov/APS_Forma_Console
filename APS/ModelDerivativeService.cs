using APS_Forma_Console.Auth;
using APS_Forma_Console.Utils;
using System.Text.Json;

namespace APS_Forma_Console.APS;
internal class ModelDerivativeService(AuthService authService)
{
    public async Task<string?> GetRevitVersionFromManifest(string urn)
    {
        JsonDocument document = await authService.GetJson(Endpoints.Manifest(urn));
        return JsonExtensions.FindPropertyRecursive(document.RootElement, "RVTVersion");
    }

    public async Task<List<RevitLinkInfo>> GetRevitLinks(string projectId, string versionId)
    {
        using JsonDocument document = await authService.GetJson(Endpoints.PublishedVersion(projectId, versionId));
        return JsonExtensions.ReadLinks(document);
    }

    public async Task<List<ViewInfo>> GetModelViews(string urn)
    {
        using JsonDocument document = await authService.GetJson(Endpoints.Metadata(urn));
        return JsonExtensions.ReadViews(document);
    }
}
