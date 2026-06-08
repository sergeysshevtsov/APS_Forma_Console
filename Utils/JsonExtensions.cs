using APS_Forma_Console.APS;
using APS_Forma_Console.Auth;
using APS_Forma_Console.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace APS_Forma_Console.Utils;
internal static class JsonExtensions
{
    public static string GetRequiredString(this JsonNode node, string path) =>
        node.GetString(path) ??
        throw new InvalidOperationException($"JSON value is missing: {path}");

    public static string? GetString(this JsonNode node, string path)
    {
        JsonNode? current = node;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            current = current?[segment];
            if (current is null)
                return null;
        }

        return current.ToString();
    }

    public static string GetDisplayName(this JsonNode node) =>
        node.GetString("attributes.displayName") ??
        node.GetString("attributes.name") ??
        node.GetString("id") ??
        "(unnamed)";

    public static List<HubInfo> ReadHubs(JsonDocument json) =>
       ReadNamedResources(json, (id, name) => new HubInfo(id, name));

    public static List<ProjectInfo> ReadProjects(JsonDocument json) =>
        ReadNamedResources(json, (id, name) => new ProjectInfo(id, name));

    public static List<FolderEntry> ReadFolderEntries(JsonDocument json, string projectId)
    {
        List<FolderEntry> entries = [];

        foreach (JsonElement item in json.RootElement.GetProperty("data").EnumerateArray())
        {
            string? id = item.GetProperty("id").GetString() ?? "";
            string? type = item.GetProperty("type").GetString() ?? "";
            string? name = GetDisplayName(item);

            FolderEntryKind kind = type.Equals("folders", StringComparison.OrdinalIgnoreCase)
                ? FolderEntryKind.Folder
                : FolderEntryKind.Item;

            entries.Add(new FolderEntry(id, name, kind, projectId));
        }

        return [.. entries.OrderBy(e => e.Kind).ThenBy(e => e.Name)];
    }

    public static string GetDisplayName(JsonElement element)
    {
        if (element.TryGetProperty("attributes", out var attributes))
        {
            if (attributes.TryGetProperty("displayName", out var displayName))
                return displayName.GetString() ?? "(unnamed)";

            if (attributes.TryGetProperty("name", out var name))
                return name.GetString() ?? "(unnamed)";
        }

        return "(unnamed)";
    }

    public static string GetType(JsonElement element)
    {
        if (element.TryGetProperty("attributes", out var attributes))
        {
            if (attributes.TryGetProperty("extension", out var extension))
            {

                if (extension.TryGetProperty("type", out var type))
                    return type.GetString() ?? "(unnamed)";
            }
        }

        return "(unnamed)";
    }

    public static string GetVersionId(JsonElement version)
    {
        if (version.TryGetProperty("id", out var id))
            return id.GetString()
                ?? throw new InvalidOperationException("Id is empty.");

        throw new InvalidOperationException("Selected file version does not contain a ID. It may not be translated yet.");
    }

    private static string? GetPropertyValue(JsonElement obj, params string[] names)
    {
        if (!obj.TryGetProperty("properties", out var props))
            return null;

        foreach (var group in props.EnumerateObject())
            foreach (var prop in group.Value.EnumerateObject())
                if (names.Any(name => prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    return ValueToString(prop.Value);

        return null;
    }

    private static string ValueToString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Number => value.ToString(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => value.ToString()
    };

    public static string GetDerivativeUrn(JsonElement version)
    {
        if (version.TryGetProperty("relationships", out var relationships) &&
            relationships.TryGetProperty("derivatives", out var derivatives) &&
            derivatives.TryGetProperty("data", out var data) &&
            data.TryGetProperty("id", out var id))
            return id.GetString()
                ?? throw new InvalidOperationException("Derivative URN is empty.");

        throw new InvalidOperationException("Selected file version does not contain a derivative URN. It may not be translated yet.");
    }

   


    private static List<T> ReadNamedResources<T>(JsonDocument json, Func<string, string, T> factory)
    {
        List<T> result = [];

        foreach (JsonElement item in json.RootElement.GetProperty("data").EnumerateArray())
        {
            string? id = item.GetProperty("id").GetString() ?? "";
            string? name = GetDisplayName(item);
            result.Add(factory(id, name));
        }

        return result;
    }

    public static async Task<JsonDocument> GetJson(this AuthService authService, string url, bool isUserIdUse = false)
    {
        using HttpClient httpClient = new();
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authService.GetToken());
        if (isUserIdUse)
            request.Headers.Add("x-user-id", authService.GetUserId());

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"GET failed: {(int)response.StatusCode} {url}{Environment.NewLine}{body}");

        return JsonDocument.Parse(body);
    }

    public static string? FindPropertyRecursive(JsonElement element, string propertyName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()
                            : property.Value.ToString();

                    string? nestedProperty = FindPropertyRecursive(property.Value, propertyName);
                    if (!string.IsNullOrEmpty(nestedProperty))
                        return nestedProperty;
                }
                break;
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string? nestedProperty = FindPropertyRecursive(item, propertyName);
                    if (!string.IsNullOrEmpty(nestedProperty))
                        return nestedProperty;
                }
                break;
            default:
                break;
        }

        return null;
    }

    public static List<RevitLinkInfo> ReadLinks(JsonDocument document)
    {
        List<RevitLinkInfo> links = [];
        var root = document.RootElement;
        if (root.TryGetProperty("hostFile", out var host))
            links.Add(new RevitLinkInfo
            (
                GetString(host, "modelName"),
                GetStringOrNull(host, "versionId"),
                true
            ));

        if (root.TryGetProperty("linkedFiles", out var linkedFiles) &&
            linkedFiles.TryGetProperty("results", out var results) &&
            results.ValueKind == JsonValueKind.Array)
            links.AddRange(
                from item in results.EnumerateArray()
                select
                    new RevitLinkInfo
                    (
                        GetString(item, "modelName"),
                        GetStringOrNull(item, "versionId"),
                        false
                    ));

        return links;
    }

    private static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ?
        property.GetString() ?? "" : "";

    private static string? GetStringOrNull(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ?
        property.GetString() : null;

    private static IEnumerable<JsonElement> EnumerateObjects(JsonDocument properties)
    {
        if (!properties.RootElement.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("collection", out var collection))
            yield break;

        foreach (var obj in collection.EnumerateArray())
            yield return obj;
    }

    public static List<ViewInfo> ReadViews(JsonDocument document)
    {
        List<ViewInfo> views = [];

        if (!document.RootElement.TryGetProperty("data", out var data) || !data.TryGetProperty("metadata", out var metadataArray))
            return views;

        foreach (var item in metadataArray.EnumerateArray())
            views.Add(new ViewInfo(
                item.GetProperty("guid").GetString() ?? "",
                item.TryGetProperty("name", out var name) ? name.GetString() ?? "(unnamed)" : "(unnamed)",
                item.TryGetProperty("role", out var role) ? role.GetString()?.ToUpper() ?? "" : ""));

        return views;
    }

    public static List<FamilyInstanceInfo> ReadFamilyInstances(JsonDocument document, int limit = 200)
    {
        List<FamilyInstanceInfo> result = [];

        foreach (JsonElement element in EnumerateObjects(document))
        {
            string? name = element.TryGetProperty("name", out var n) ? n.GetString() ?? "(unnamed)" : "(unnamed)";
            var nameData = name.Split(" ");
            if (nameData.Length == 2 && nameData[1].Contains('['))
            {
                long objectId = element.TryGetProperty("objectid", out var id) && id.TryGetInt64(out var parsedId) ? parsedId : 0;
                element.TryGetProperty("properties", out var properties);
                
                properties.TryGetProperty("Identity Data", out var identityData);
                string typeName = identityData.TryGetProperty("Type Name", out var n1) ? n1.GetString() ?? "(unnamed)" : "(unnamed)";
                
                properties.TryGetProperty("Constraints", out var constraints);
                string host = constraints.TryGetProperty("Host", out var n2) ? n2.GetString() ?? "(unnamed)" : "(unnamed)";
               
                result.Add(new FamilyInstanceInfo(objectId, name, "", nameData[0], typeName, nameData[1], host));
            }

            if (result.Count >= limit)
                break;
        }

        return result;
    }
}
