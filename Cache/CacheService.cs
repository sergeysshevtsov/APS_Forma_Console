using APS_Forma_Console.Auth;
using APS_Forma_Console.Models;
using System.Text.Json;

namespace APS_Forma_Console.Cache;
internal class CacheService
{
    private string userDirectory = string.Empty;
    private string cacheFilePath = string.Empty;

    public CacheService()
    {
        userDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aps-forma-console");
        if (!Directory.Exists(userDirectory))
            Directory.CreateDirectory(userDirectory);
        cacheFilePath = Path.Combine(userDirectory, "cache.json");
        if (!File.Exists(cacheFilePath))
            File.Create(cacheFilePath);
    }

    public SelectedFileCacheInfo? CacheFileCheck()
    {
        if (!File.Exists(cacheFilePath))
            return null;

        SelectedFileCacheInfo? selectedFileCacheInfo = null;
        try
        {
            string json = File.ReadAllText(cacheFilePath);
            selectedFileCacheInfo = JsonSerializer.Deserialize<SelectedFileCacheInfo>(json);
        }
        catch { }
        return selectedFileCacheInfo;
    }

    public void WriteCacheFile(SelectedFileCacheInfo selectedFileCacheInfo) =>
        File.WriteAllText(cacheFilePath, JsonSerializer.Serialize(selectedFileCacheInfo));
    
    
}
