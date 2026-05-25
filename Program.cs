using APS_Forma_Console.Auth;
using APS_Forma_Console.Config;
using APS_Forma_Console.Enums;
using APS_Forma_Console.Utils;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.Title = "Forma console browser (via APS)";

        IConfiguration configuration = new ConfigurationBuilder()
          .SetBasePath(AppContext.BaseDirectory)
          .AddJsonFile("appsettings.json", true, false)
          .AddUserSecrets<Program>(true)
          .Build();

        APSConfig? config = 
            configuration.GetSection("APSConfig").Get<APSConfig>() ??
            throw new InvalidOperationException("APS configuration section is missing.");
        ConfigValidation(config);

        AuthService auth = new(config);
        await auth.Login();

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void ConfigValidation(APSConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ClientId))
            throw new InvalidOperationException("APS:ClientId is missing.");
        else
            Common.ConsoleWriteLine(string.Concat("ClientId: ", config.ClientId.MaskString(2)), ConsoleTextType.Success);

        if (string.IsNullOrWhiteSpace(config.ClientSecret))
            throw new InvalidOperationException("APS:ClientSecret is missing.");
        else
            Common.ConsoleWriteLine(string.Concat("ClientSecret: ", config.ClientSecret.MaskString(2)), ConsoleTextType.Success);

        if (string.IsNullOrWhiteSpace(config.CallbackUrl))
            throw new InvalidOperationException("APS:CallbackUrl is missing.");
        else
            Common.ConsoleWriteLine($"Callback URL: {config.CallbackUrl}", ConsoleTextType.Success);

        if (config.Scopes is null || config.Scopes.Count == 0)
            throw new InvalidOperationException("APS scopes are missing.");
    }
}