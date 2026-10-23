using Microsoft.AspNetCore.Mvc;
using SpotifyWrapped.Services;

namespace SpotifyWrapped.Controllers;

public class StatsController : Controller
{
    private readonly SpotifyClient _spotify;

    public StatsController(SpotifyClient spotify) => _spotify = spotify;

    [HttpGet("/test")]
    public async Task<IActionResult> Test() =>
        Json(await _spotify.GetRecentlyPlayedAsync(null));
}