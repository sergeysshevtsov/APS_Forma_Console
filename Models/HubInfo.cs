using APS_Forma_Console.Utils;
using System.Text.Json.Nodes;

namespace APS_Forma_Console.Models;
internal record HubInfo(string Id, string Name)
{
    public static HubInfo FromJson(JsonNode node) =>
        new(
            node.GetRequiredString("id"),
            node.GetDisplayName()
        );
}
