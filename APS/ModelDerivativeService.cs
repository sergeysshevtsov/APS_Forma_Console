using APS_Forma_Console.Auth;
using APS_Forma_Console.Utils;
using System;
using System.Text.Json;
using System.Xml.Linq;

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
        using JsonDocument document = await authService.GetJson(Endpoints.PublishedVersion(projectId, versionId), true);
        //File.WriteAllText("C:\\Temp\\links.json", document.RootElement.GetRawText());
        return JsonExtensions.ReadLinks(document);
    }

    public async Task<List<ViewInfo>> GetModelViews(string urn)
    {
        using JsonDocument document = await authService.GetJson(Endpoints.Metadata(urn));
        //File.WriteAllText("C:\\Temp\\views.json", document.RootElement.GetRawText());
        return JsonExtensions.ReadViews(document);
    }

    public async Task<List<FamilyInstanceInfo>> GetModelFamilyInstances(string urn, string viewGuid)
    {
        using JsonDocument document = await authService.GetJson(Endpoints.ModelProperties(urn, viewGuid));
        File.WriteAllText("C:\\Temp\\familyInstances.json", document.RootElement.GetRawText());
        return JsonExtensions.ReadFamilyInstances(document);
    }

    public async Task<string> GetDefault3DViewGuid(string urn)
    {
        using JsonDocument metadata = await authService.GetJson(Endpoints.Metadata(urn));
        List<ViewInfo> views = await GetModelViews(urn);

        ViewInfo view3D = views.FirstOrDefault(v => v.Role.Equals("3d", StringComparison.OrdinalIgnoreCase))
            ?? views.FirstOrDefault()
            ?? throw new InvalidOperationException("No derivative metadata view/model GUID was found.");

        return view3D.Guid;
    }
}
