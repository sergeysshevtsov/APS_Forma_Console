namespace APS_Forma_Console.Models;
internal enum FolderEntryKind
{
    Folder,
    Item
}

internal sealed record FolderEntry(
    string Id,
    string Name,
    FolderEntryKind Kind,
    string ProjectId)
{
    public bool IsRevitFile =>
        Kind == FolderEntryKind.Item &&
        Name.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase);
}
