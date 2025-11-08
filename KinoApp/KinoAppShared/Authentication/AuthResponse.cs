namespace KinoAppShared.Authentication;
public record AuthResponse(string AccessToken, DateTime ExpiresUtc, string Username);
