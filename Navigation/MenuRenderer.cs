using APS_Forma_Console.APS;
using APS_Forma_Console.Auth;
using APS_Forma_Console.Cache;
using APS_Forma_Console.Items;
using APS_Forma_Console.Models;
using APS_Forma_Console.Utils;
using System;
using System.Text.Json;

namespace APS_Forma_Console.Navigation;
internal class MenuRenderer(CacheService cacheService, DataManagementService dataManagementService, ModelDerivativeService modelDerivativeService)
{
    SelectedFileCacheInfo? selectedFileCacheInfo = null;

    private readonly List<string> menuItems = [
            "Select Revit file from Forma",
            "Show Revit file version",
            "Show model links",
            "Show model views",
            "Show model family instances"
        ];

    public async Task MainMenu()
    {
        selectedFileCacheInfo = cacheService.GetCacheFile();
        bool result = true;
        while (result)
        {
            if (selectedFileCacheInfo == null)
            {
                MenuExtension.MenuHeader("No selected file");
                result = await RunItemSelection(MenuExtension.MenuItemsRender([menuItems[0]], false, true));
            }
            else
            {
                MenuExtension.MenuHeader("Selected file Info");
                ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.ProjectName, Enums.ConsoleTextType.Info);
                ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.FolderName, Enums.ConsoleTextType.Info);
                ConsoleExtension.ConsoleWriteLine(selectedFileCacheInfo?.ItemName, Enums.ConsoleTextType.Info);

                result = await RunItemSelection(MenuExtension.MenuItemsRender(menuItems, false, true));
            }
        }
    }

    private async Task<bool> RunItemSelection(int itemNumber)
    {
        bool selected = true;
        switch (itemNumber)
        {
            case 1:
                Console.Clear();
                SelectRevitFile selectRevitFile = new(this, cacheService, dataManagementService);
                await selectRevitFile.Start();
                break;
            case 2:
                {
                    MenuExtension.MenuHeader("Revit file version");
                    JsonElement element = await dataManagementService.GetLatestVersion(selectedFileCacheInfo?.ProjectId, selectedFileCacheInfo?.ItemId);
                    string? derivativeUrn = JsonExtensions.GetDerivativeUrn(element);
                    string? version = await modelDerivativeService.GetRevitVersionFromManifest(derivativeUrn) ?? string.Empty;
                    ConsoleExtension.ConsoleWriteLine(version, Enums.ConsoleTextType.Success);
                    break;
                }
            case 3:
                {
                    MenuExtension.MenuHeader("Model's links list");
                    JsonElement element = await dataManagementService.GetLatestVersion(selectedFileCacheInfo?.ProjectId, selectedFileCacheInfo?.ItemId);
                    string? versionId = JsonExtensions.GetVersionId(element);
                    List<RevitLinkInfo> links = await modelDerivativeService.GetRevitLinks(selectedFileCacheInfo?.ProjectId, Uri.EscapeDataString(versionId));
                    if (links.Count == 0)
                        Console.WriteLine("No links found.");
                    else
                    {
                        foreach (RevitLinkInfo link in links)
                        {
                            ConsoleExtension.ConsoleWriteLine($"- {link.Name}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Version: {link.VersionId}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Is host: {link.IsHost}", Enums.ConsoleTextType.Success);
                        }
                    }
                    break;
                }
            case 4:
                {
                    MenuExtension.MenuHeader("Model's views list");
                    JsonElement element = await dataManagementService.GetLatestVersion(selectedFileCacheInfo?.ProjectId, selectedFileCacheInfo?.ItemId);
                    string? derivativeUrn = JsonExtensions.GetDerivativeUrn(element);
                    List<ViewInfo> views = await modelDerivativeService.GetModelViews(derivativeUrn);
                    if (views.Count == 0)
                        Console.WriteLine("No views found.");
                    else
                    {
                        foreach (ViewInfo view in views)
                        {
                            ConsoleExtension.ConsoleWriteLine($"- {view.Name}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Guid: {view.Guid}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Role: {view.Role}", Enums.ConsoleTextType.Success);
                        }
                    }
                    break;
                }
            case 5:
                {
                    MenuExtension.MenuHeader("Model's family instances list");
                    JsonElement element = await dataManagementService.GetLatestVersion(selectedFileCacheInfo?.ProjectId, selectedFileCacheInfo?.ItemId);
                    string? derivativeUrn = JsonExtensions.GetDerivativeUrn(element);
                    string? viewGuid = await modelDerivativeService.GetDefault3DViewGuid(derivativeUrn);
                    List<FamilyInstanceInfo> familyInstances = await modelDerivativeService.GetModelFamilyInstances(derivativeUrn, viewGuid);
                    if (familyInstances.Count == 0)
                        Console.WriteLine("No family instances found.");
                    else
                    {
                        foreach (FamilyInstanceInfo fi in familyInstances)
                        {
                            ConsoleExtension.ConsoleWriteLine($"- {fi.Name}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  ObjectId: {fi.ObjectId}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Category: {fi.Category}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Family: {fi.Family}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Id: {fi.IdInt}", Enums.ConsoleTextType.Success);
                            ConsoleExtension.ConsoleWriteLine($"  Host: {fi.Host}", Enums.ConsoleTextType.Success);
                        }
                    }
                    break;
                }
            case 0:
                selected = false;
                break;
        }

        return selected;
    }
}
