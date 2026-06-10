using APS_Forma_Console.APS;
using APS_Forma_Console.Cache;
using APS_Forma_Console.Models;
using APS_Forma_Console.Navigation;
using APS_Forma_Console.Utils;
using System.Text.Json;

namespace APS_Forma_Console.Items;
internal class SelectRevitFile
{
    private readonly MenuRenderer menuRenderer;
    private readonly CacheService cacheService;
    private readonly DataManagementService dataManagementService;
    private SelectedFileCacheInfo selectedFileCacheInfo;

    public SelectRevitFile(MenuRenderer menuRenderer, CacheService cacheService, DataManagementService dataManagementService)
    {
        this.menuRenderer = menuRenderer;
        this.cacheService = cacheService;
        this.dataManagementService = dataManagementService;
        this.selectedFileCacheInfo = new();
    }

    public async Task Start() 
    {
        using JsonDocument hubsJson = await dataManagementService.GetHubs();
        List<HubInfo> hubs = JsonExtensions.ReadHubs(hubsJson);
        if (hubs.Count == 0)
        {
            Console.WriteLine("No hubs found for the signed-in user.");
            return;
        }
        
        HubInfo hub = hubs[RenderHubs(hubs) - 1];
        selectedFileCacheInfo.HubId = hub.Id;
        selectedFileCacheInfo.HubName = hub.Name;

        using JsonDocument projectsJson = await dataManagementService.GetProjects(hub.Id);
        var projects = JsonExtensions.ReadProjects(projectsJson);
        if (projects.Count == 0)
        {
            Console.WriteLine("No projects found in this hub.");
            return;
        }
        
        ProjectInfo project = projects[RenderProjects(projects) - 1];
        selectedFileCacheInfo.ProjectId = project.Id;
        selectedFileCacheInfo.ProjectName = project.Name;

        using JsonDocument topFoldersJson = await dataManagementService.GetTopFolders(hub.Id, project.Id);
        var topFolders = JsonExtensions.ReadFolderEntries(topFoldersJson, project.Id)
            .Where(x => x.Kind == FolderEntryKind.Folder)
            .ToList();
        if (topFolders.Count == 0)
        {
            Console.WriteLine("No top folders found in this project.");
            return;
        }
        RenderFolderEntries(selectedFileCacheInfo.ProjectName, topFolders);
        FolderEntry folder = topFolders[MenuExtension.ReadSelection(1, topFolders.Count) - 1];
        selectedFileCacheInfo.FolderId = folder.Id;
        selectedFileCacheInfo.FolderName = folder.Name;

        ConsoleNavigator navigator = new();
        navigator.SetRootFolder(folder);
        await BrowseCurrentFolder(project.Id, navigator);
    }

    public static int RenderHubs(IReadOnlyList<HubInfo> hubs)
    {
        MenuExtension.MenuHeader("Available hubs");
        return MenuExtension.MenuItemsRender([.. hubs.Select(h => h.Name)]);
    }

    public int RenderProjects(IReadOnlyList<ProjectInfo> projects)
    {
        MenuExtension.MenuHeader(selectedFileCacheInfo.HubName);
        return MenuExtension.MenuItemsRender([.. projects.Select(h => h.Name)]);
    }

    public void RenderFolderEntries(string folderName, IReadOnlyList<FolderEntry> entries, bool canGoBack = false)
    {
        MenuExtension.MenuHeader(folderName);
        for (var i = 0; i < entries.Count; i++)
        {
            var prefix = entries[i].Kind == FolderEntryKind.Folder ? "[Folder]" : "[File]";
            Console.WriteLine($"{i + 1}. {prefix} {entries[i].Name}");
        }

        if (canGoBack)
            Console.WriteLine($"{entries.Count + 1}. Back");
    }

    private async Task BrowseCurrentFolder(string projectId, ConsoleNavigator navigator)
    {
        while (navigator.CurrentFolder is not null)
        {
            using JsonDocument contentsJson = await dataManagementService.GetFolderContents(projectId, navigator.CurrentFolder.Id);
            List<FolderEntry> entries = JsonExtensions.ReadFolderEntries(contentsJson, projectId);
            int maxSelection = entries.Count /*+ (navigator.CanGoBack ? 1 : 0)*/;

            if (maxSelection == 0)
            {
                Console.WriteLine("Folder is empty.");
                return;
            }

            RenderFolderEntries(selectedFileCacheInfo.FolderName, entries);
            int selection = MenuExtension.ReadSelection(1, maxSelection);
            if (navigator.CanGoBack && selection == entries.Count + 1)
            {
                navigator.Back();
                continue;
            }

            var selected = entries[selection - 1];
            if (selected.Kind == FolderEntryKind.Folder)
            {
                navigator.EnterFolder(selected);
                selectedFileCacheInfo.FolderId = selected.Id;
                selectedFileCacheInfo.FolderName += string.Concat("/", selected.Name);
                continue;
            }

            if (!selected.IsRevitFile)
            {
                Console.WriteLine("Only Revit .rvt files open the detail menu in this sample.");
                continue;
            }

            await Select(projectId, selected);
        }
    }

    private async Task Select(string projectId, FolderEntry selectedItem)
    {
        JsonElement latestVersion = await dataManagementService.GetLatestVersion(projectId, selectedItem.Id);
        var derivativeUrn = JsonExtensions.GetDerivativeUrn(latestVersion);

        MenuExtension.MenuHeader("Selected Revit file");
        ConsoleExtension.ConsoleWriteLine($"Name: {selectedItem.Name}", Enums.ConsoleTextType.Info);
        ConsoleExtension.ConsoleWriteLine($"Item ID: {selectedItem.Id}", Enums.ConsoleTextType.Info);
        ConsoleExtension.ConsoleWriteLine($"Version ID: {latestVersion.GetProperty("id").GetString()}", Enums.ConsoleTextType.Info);
        ConsoleExtension.ConsoleWriteLine($"Derivative URN: {derivativeUrn}", Enums.ConsoleTextType.Info);

        selectedFileCacheInfo.ItemId = selectedItem.Id;
        selectedFileCacheInfo.ItemName = selectedItem.Name;

        selectedFileCacheInfo.ModelGuid = JsonExtensions.GetModelGuid(latestVersion);
        selectedFileCacheInfo.ProjectGuid = JsonExtensions.GetProjectGuid(latestVersion);

        cacheService.WriteCacheFile(selectedFileCacheInfo);
        await menuRenderer.MainMenu();
    }
}
