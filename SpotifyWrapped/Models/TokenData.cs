namespace SpotifyWrapped.Models;

public record TokenData(string AccessToken, string RefreshToken, DateTime ExpiresAt);