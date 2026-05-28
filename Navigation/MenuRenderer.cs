using APS_Forma_Console.APS;
using APS_Forma_Console.Auth;
using APS_Forma_Console.Cache;
using APS_Forma_Console.Items;
using APS_Forma_Console.Models;
using APS_Forma_Console.Utils;
using System.Text.Json;

namespace APS_Forma_Console.Navigation;
internal class MenuRenderer
{
    private readonly CacheService cacheService;
    private readonly DataManagementService dataManagementService;
    private readonly List<string> menuItems;
    private readonly string exit = "Exit";

    public MenuRenderer(CacheService cacheService, DataManagementService dataManagementService)
    {
        this.cacheService = cacheService;
        this.dataManagementService = dataManagementService;
        menuItems = new List<string>() {
            "Select Revit file from Forma",
            "Check views",
            ""
        };
    }

    public async Task MainMenu()
    {
        SelectedFileCacheInfo? selectedFileCacheInfo = cacheService.CacheFileCheck();
        int selectedItemNumber = -1;
        if (selectedFileCacheInfo == null)
        {
            MenuExtension.MenuHeader("No selected file");
            await RunItemSelection(MenuExtension.MenuItemsRender([menuItems[0]], false));
        }
        else
        {
            MenuExtension.MenuHeader("Selected file Info");
            ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.ProjectName, Enums.ConsoleTextType.Info);
            ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.FolderName, Enums.ConsoleTextType.Info);
            ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.ItemName, Enums.ConsoleTextType.Info);

            Console.WriteLine("1. Select another Revit file from Forma");
            Console.WriteLine("2. ");
            Console.WriteLine("3. ");
            Console.WriteLine("4. ");
            Console.WriteLine("5. ");
            Console.WriteLine("0. Exit");
        }
    }

    private async Task RunItemSelection(int itemNumber) 
    {
        switch (itemNumber)
        {
            case 1:
                Console.Clear();
                SelectRevitFile selectRevitFile = new(this, cacheService, dataManagementService);
                await selectRevitFile.Start();
                break;
        }
    }
}
