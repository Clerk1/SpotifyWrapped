using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyWrapped.Data;

namespace SpotifyWrapped.Controllers;

[ApiController]
public class StatsController : ControllerBase
{
    private readonly AppDb _db;

    public StatsController(AppDb db) => _db = db;

    [HttpGet("/stats/top-artists")]
    public async Task<IActionResult> TopArtists() =>
        Ok(await _db.Plays
            .GroupBy(p => p.ArtistName)
            .Select(g => new
            {
                Artist = g.Key,
                Plays = g.Count(),
                Minutes = g.Sum(p => p.DurationMs) / 60000
            })
            .OrderByDescending(x => x.Plays)
            .Take(10)
            .ToListAsync());
}