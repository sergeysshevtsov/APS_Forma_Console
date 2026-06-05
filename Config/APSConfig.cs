namespace APS_Forma_Console.Config;
internal class APSConfig
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public string CallbackUrl { get; init; } = "http://localhost:8080/callback/";
    public required List<string> Scopes { get; init; }
    public required string Region { get; init; }
    public string NormalizedCallbackUrl => CallbackUrl.EndsWith('/') ? CallbackUrl : string.Concat(CallbackUrl, "/");
    public string? UserId {  get; init; }
}
