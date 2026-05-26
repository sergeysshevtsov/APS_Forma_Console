using APS_Forma_Console.APS;
using APS_Forma_Console.Models;
using APS_Forma_Console.Navigation;
using APS_Forma_Console.Utils;
using System.Text.Json;

namespace APS_Forma_Console.Items;
internal class SelectRevitFile
{
    private readonly DataManagementService dataManagementService;
    private SelectedFileCacheInfo selectedFileCacheInfo;

    public SelectRevitFile(DataManagementService dataManagementService)
    {
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
        RenderProjects(projects);
        ProjectInfo project = projects[MenuExtension.ReadSelection(1, projects.Count) - 1];
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
        RenderFolderEntries(topFolders, false);
        FolderEntry folder = topFolders[MenuExtension.ReadSelection(1, topFolders.Count) - 1];
        selectedFileCacheInfo.FolderId = folder.Id;
        selectedFileCacheInfo.FolderName = folder.Name;

        //navigator.SetRootFolder(rootFolder);
        await BrowseCurrentFolderc(project.Id);
    }

    public static int RenderHubs(IReadOnlyList<HubInfo> hubs)
    {
        MenuExtension.MenuHeader("Available hubs");
        return MenuExtension.MenuItemsRender([.. hubs.Select(h => h.Name)]);
    }

    public static void RenderProjects(IReadOnlyList<ProjectInfo> projects)
    {
        MenuExtension.MenuHeader("Available projects");
        MenuExtension.MenuItemsRender([.. projects.Select(h => h.Name)]);
    }

    public static void RenderFolderEntries(IReadOnlyList<FolderEntry> entries, bool canGoBack)
    {
        MenuExtension.MenuHeader("Folder contents");
        for (var i = 0; i < entries.Count; i++)
        {
            var prefix = entries[i].Kind == FolderEntryKind.Folder ? "[Folder]" : "[File]";
            Console.WriteLine($"{i + 1}. {prefix} {entries[i].Name}");
        }

        if (canGoBack)
            Console.WriteLine($"{entries.Count + 1}. Back");
    }

    private async Task BrowseCurrentFolderc(string projectId)
    {
        while (navigator.CurrentFolder is not null)
        {
            using var contentsJson = await dataClient.GetFolderContents(projectId, navigator.CurrentFolder.Id);
            List<FolderEntry> entries = JsonExtensions.ReadFolderEntries(contentsJson, projectId);
            int maxSelection = entries.Count + (navigator.CanGoBack ? 1 : 0);

            if (maxSelection == 0)
            {
                Console.WriteLine("Folder is empty.");
                return;
            }

            MenuRenderer.RenderFolderEntries(entries, navigator.CanGoBack);
            int selection = SelectionReader.ReadSelection(1, maxSelection);
            if (navigator.CanGoBack && selection == entries.Count + 1)
            {
                navigator.Back();
                continue;
            }

            var selected = entries[selection - 1];
            if (selected.Kind == FolderEntryKind.Folder)
            {
                navigator.EnterFolder(selected);
                continue;
            }

            if (!selected.IsRevitFile)
            {
                Console.WriteLine("Only Revit .rvt files open the detail menu in this sample.");
                continue;
            }

            await OpenRevitFile(projectId, selected);
        }
    }
}
