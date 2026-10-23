namespace SpotifyWrapped.Services;

using SpotifyWrapped.Models;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

public class SpotifyClient
{
    private readonly HttpClient _http;
    private readonly SpotifyAuth _auth;

    public SpotifyClient(IHttpClientFactory factory, SpotifyAuth auth)
    {
        _http = factory.CreateClient();
        _auth = auth;
    }

    public async Task<List<Play>> GetRecentlyPlayedAsync(DateTime? after, CancellationToken ct = default)
    {
        var url = "https://api.spotify.com/v1/me/player/recently-played?limit=50";
        if (after.HasValue)
        {
            var utc = DateTime.SpecifyKind(after.Value, DateTimeKind.Utc);
            url += "&after=" + new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _auth.GetAccessTokenAsync(ct));

        var res = await _http.SendAsync(req, ct);

        if (res.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var wait = res.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(30);
            throw new HttpRequestException($"Rate limit ramt. Prøv igen om {wait.TotalSeconds:0} sek.");
        }
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var plays = new List<Play>();

        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
        {
            var track = item.GetProperty("track");
            var artist = track.GetProperty("artists")[0];

            plays.Add(new Play
            {
                TrackId = track.GetProperty("id").GetString() ?? "",
                TrackName = track.GetProperty("name").GetString() ?? "",
                ArtistId = artist.GetProperty("id").GetString() ?? "",
                ArtistName = artist.GetProperty("name").GetString() ?? "",
                AlbumName = track.GetProperty("album").GetProperty("name").GetString() ?? "",
                DurationMs = track.GetProperty("duration_ms").GetInt32(),
                PlayedAt = DateTimeOffset.Parse(item.GetProperty("played_at").GetString()!).UtcDateTime
            });
        }
        return plays;
    }
}