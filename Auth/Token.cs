namespace APS_Forma_Console.Auth;
public record Token(string access_token, string token_type, int expires_in, string scope);