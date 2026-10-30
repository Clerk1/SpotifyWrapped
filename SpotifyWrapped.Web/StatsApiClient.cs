using System.Net.Http.Json;

namespace SpotifyWrapped.Web;

public record TopArtistDto(string Artist, int Plays, int Minutes);

public class StatsApiClient(HttpClient http)
{
    public Task<List<TopArtistDto>?> GetTopArtistsAsync() =>
        http.GetFromJsonAsync<List<TopArtistDto>>("/stats/top-artists");
}