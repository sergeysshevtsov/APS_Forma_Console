namespace APS_Forma_Console.Models;
internal class SelectedFileCacheInfo
{
    public SelectedFileCacheInfo()
    {
        ProjectId = string.Empty;
        ProjectName = string.Empty;
        FolderId = string.Empty;
        FolderName = string.Empty;
        ItemId = string.Empty;
        ItemName = string.Empty;
    }

    public string? ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public string? FolderId { get; init; }
    public string? FolderName { get; init; }
    public string? ItemId { get; init; }
    public string? ItemName { get; init; }
}
