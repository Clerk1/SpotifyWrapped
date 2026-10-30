namespace SpotifyWrapped.Models;

public class Play
{
    public int Id { get; set; }
    public string TrackId { get; set; } = "";
    public string TrackName { get; set; } = "";
    public string ArtistId { get; set; } = "";
    public string ArtistName { get; set; } = "";
    public string AlbumName { get; set; } = "";
    public int DurationMs { get; set; }
    public DateTime PlayedAt { get; set; }
}