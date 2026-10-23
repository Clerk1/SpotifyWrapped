using Microsoft.AspNetCore.Mvc;
using SpotifyWrapped.Services;

namespace SpotifyWrapped.Controllers;

public class AuthController : Controller
{
    private readonly SpotifyAuth _auth;

    public AuthController(SpotifyAuth auth) => _auth = auth;

    [HttpGet("/login")]
    public IActionResult Login() => Redirect(_auth.BuildLoginUrl());

    [HttpGet("/callback")]
    public async Task<IActionResult> Callback(string? code, string? state, string? error)
    {
        if (error != null || code == null || state == null)
            return BadRequest($"Login fejlede: {error}");

        await _auth.HandleCallbackAsync(code, state);
        return Redirect("/test");
    }
}