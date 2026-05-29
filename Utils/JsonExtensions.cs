using APS_Forma_Console.APS;
using APS_Forma_Console.Auth;
using APS_Forma_Console.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    public static async Task<JsonDocument> GetJson(this AuthService authService, string url)
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


}
