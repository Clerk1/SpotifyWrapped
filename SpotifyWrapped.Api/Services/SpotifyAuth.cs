namespace SpotifyWrapped.Services;

using SpotifyWrapped.Models;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

public class SpotifyAuth
{
    private readonly SpotifyOptions _opt;
    private readonly HttpClient _http;
    private readonly TokenStore _store;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _verifier;
    private string? _state;
    private TokenData? _token;

    public SpotifyAuth(IOptions<SpotifyOptions> opt, IHttpClientFactory factory, TokenStore store)
    {
        _opt = opt.Value;
        _http = factory.CreateClient();
        _store = store;
        _token = store.Load();
    }

    public bool IsLoggedIn => _token != null;

    public string BuildLoginUrl()
    {
        _verifier = Base64Url(RandomNumberGenerator.GetBytes(64));
        _state = Base64Url(RandomNumberGenerator.GetBytes(16));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(_verifier)));

        var query = new Dictionary<string, string>
        {
            ["client_id"] = _opt.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = _opt.RedirectUri,
            ["scope"] = "user-read-recently-played",
            ["code_challenge_method"] = "S256",
            ["code_challenge"] = challenge,
            ["state"] = _state
        };

        return "https://accounts.spotify.com/authorize?" +
               string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
    }

    public async Task HandleCallbackAsync(string code, string state)
    {
        if (_verifier is null || state != _state)
            throw new InvalidOperationException("Ugyldig state eller manglende login-forsøg.");

        await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _opt.RedirectUri,
            ["client_id"] = _opt.ClientId,
            ["code_verifier"] = _verifier
        });
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_token is null)
                throw new InvalidOperationException("Ikke logget ind. Besøg /login først.");

            if (_token.ExpiresAt <= DateTime.UtcNow.AddMinutes(1))
            {
                await RequestTokenAsync(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = _token.RefreshToken,
                    ["client_id"] = _opt.ClientId
                }, ct);
            }
            return _token!.AccessToken;
        }
        finally { _lock.Release(); }
    }

    private async Task RequestTokenAsync(Dictionary<string, string> form, CancellationToken ct = default)
    {
        var res = await _http.PostAsync("https://accounts.spotify.com/api/token",
            new FormUrlEncodedContent(form), ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Spotify token-fejl {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync(ct)}");

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        // Ved refresh sender Spotify ikke altid et nyt refresh token med
        var refresh = root.TryGetProperty("refresh_token", out var r)
            ? r.GetString()!
            : _token!.RefreshToken;

        _token = new TokenData(
            root.GetProperty("access_token").GetString()!,
            refresh,
            DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32()));

        _store.Save(_token);
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

}