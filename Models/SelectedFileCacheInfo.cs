namespace APS_Forma_Console.Models;
internal class SelectedFileCacheInfo
{
    public SelectedFileCacheInfo()
    {
        HubId = string.Empty;
        HubName = string.Empty;

        ProjectId = string.Empty;
        ProjectName = string.Empty;

        FolderId = string.Empty;
        FolderName = string.Empty;

        ItemId = string.Empty;
        ItemName = string.Empty;
    }

    public string HubId { get; set; }
    public string HubName { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string FolderId { get; set; }
    public string FolderName { get; set; }
    public string ItemId { get; set; }
    public string ItemName { get; set; }
}
