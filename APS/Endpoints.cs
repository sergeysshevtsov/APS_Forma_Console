using System.Text.Json;

namespace APS_Forma_Console.APS;
internal class Endpoints
{
    public static string Hubs() =>
        "https://developer.api.autodesk.com/project/v1/hubs";

    public static string Projects(string hubId) =>
        $"https://developer.api.autodesk.com/project/v1/hubs/{hubId}/projects";

    public static string TopFolders(string hubId, string projectId) =>
        $"https://developer.api.autodesk.com/project/v1/hubs/{Escape(hubId)}/projects/{Escape(projectId)}/topFolders";

    public static string FolderContents(string projectId, string folderId) =>
        $"https://developer.api.autodesk.com/data/v1/projects/{Escape(projectId)}/folders/{Escape(folderId)}/contents";

    public static string ItemVersions(string projectId, string itemId) =>
        $"https://developer.api.autodesk.com/data/v1/projects/{Escape(projectId)}/items/{Escape(itemId)}/versions";

    private static string Escape(string value) => Uri.EscapeDataString(value);

    public static string Manifest(string urn) =>
        $"https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest";

    public static string Metadata(string urn) =>
        $"https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/metadata";
}
