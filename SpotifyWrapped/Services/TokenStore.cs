namespace SpotifyWrapped.Services;

using SpotifyWrapped.Models;

using System.Text.Json;

public class TokenStore
{
    private const string FilePath = "tokens.json";

    public TokenData? Load() =>
        File.Exists(FilePath)
            ? JsonSerializer.Deserialize<TokenData>(File.ReadAllText(FilePath))
            : null;

    public void Save(TokenData token) =>
        File.WriteAllText(FilePath, JsonSerializer.Serialize(token));
}