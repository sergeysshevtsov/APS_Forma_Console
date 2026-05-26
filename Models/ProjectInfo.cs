using APS_Forma_Console.Utils;
using System.Text.Json.Nodes;

namespace APS_Forma_Console.Models;
internal record ProjectInfo(string Id, string Name)
{
    public static ProjectInfo FromJson(JsonNode node) =>
        new(
            node.GetRequiredString("id"),
            node.GetDisplayName()
        );
}
