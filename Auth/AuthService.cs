using APS_Forma_Console.Config;
using APS_Forma_Console.Enums;
using APS_Forma_Console.Utils;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APS_Forma_Console.Auth;
internal class AuthService
{
    private readonly APSConfig config;
    private readonly HttpClient httpClient;

    public AuthService(APSConfig config)
    {
        this.config = config;
        this.httpClient = ConfigHttpClient(new() { BaseAddress = new Uri("https://developer.api.autodesk.com/") });
    }

    public async Task<string?> Login()
    {
        Common.ConsoleWriteLine("Logging in...");
        KeyValuePair<string, string>[] form =
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", string.Join(" ", config.Scopes))
        ];

        using FormUrlEncodedContent content = new(form);

        HttpResponseMessage response = await httpClient.PostAsync("authentication/v2/token", content).ConfigureAwait(false);
        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"APS token request failed ({(int)response.StatusCode}): {body}");

        Token? token = JsonSerializer.Deserialize<Token>(body);
        string accessToken = token?.access_token ?? string.Empty;
        if (string.IsNullOrEmpty(accessToken))
            return null;
        
        Common.ConsoleWriteLine($"access_token:{token?.access_token}", ConsoleTextType.Success);
        Common.ConsoleWriteLine($"token_type: {token?.token_type}", ConsoleTextType.Success);
        Common.ConsoleWriteLine($"expires_in: {token?.expires_in}", ConsoleTextType.Success);
        return accessToken;
    }

    private HttpClient ConfigHttpClient(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", $"{config.ClientId}:{config.ClientSecret}".ToBase64Url());
        httpClient.DefaultRequestHeaders.Add("x-ads-region", config.Region);
        return httpClient;
    }
}
