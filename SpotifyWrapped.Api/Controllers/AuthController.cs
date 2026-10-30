using Microsoft.AspNetCore.Mvc;
using SpotifyWrapped.Services;

namespace SpotifyWrapped.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly SpotifyAuth _auth;
    private readonly string _frontendUrl;

    public AuthController(SpotifyAuth auth, IConfiguration config)
    {
        _auth = auth;
        _frontendUrl = config["FrontendUrl"] ?? "http://127.0.0.1:5001";
    }

    [HttpGet("/login")]
    public IActionResult Login() => Redirect(_auth.BuildLoginUrl());

    [HttpGet("/callback")]
    public async Task<IActionResult> Callback(string? code, string? state, string? error)
    {
        if (error != null || code == null || state == null)
            return BadRequest($"Login fejlede: {error}");

        await _auth.HandleCallbackAsync(code, state);
        return Redirect(_frontendUrl);   // tilbage til Blazor-appen
    }

    [HttpGet("/auth/status")]
    public IActionResult Status() => Ok(new { loggedIn = _auth.IsLoggedIn });
}