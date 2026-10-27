using Microsoft.EntityFrameworkCore;
using SpotifyWrapped.Data;

namespace SpotifyWrapped.Services;

public class PollingService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(20);

    private readonly IServiceScopeFactory _scopes;
    private readonly SpotifyAuth _auth;
    private readonly SpotifyClient _spotify;
    private readonly ILogger<PollingService> _logger;

    public PollingService(IServiceScopeFactory scopes, SpotifyAuth auth,
                          SpotifyClient spotify, ILogger<PollingService> logger)
    {
        _scopes = scopes;
        _auth = auth;
        _spotify = spotify;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Vent på, at du har logget ind via /login
            if (!_auth.IsLoggedIn)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                continue;
            }

            try
            {
                await PollOnceAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling fejlede");
            }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task PollOnceAsync(CancellationToken ct)
    {
        // AppDb er scoped, så vi opretter et scope manuelt
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();

        var latest = await db.Plays.MaxAsync(p => (DateTime?)p.PlayedAt, ct);
        var items = await _spotify.GetRecentlyPlayedAsync(latest, ct);

        var added = 0;
        foreach (var item in items)
        {
            var exists = await db.Plays.AnyAsync(
                p => p.TrackId == item.TrackId && p.PlayedAt == item.PlayedAt, ct);

            if (!exists)
            {
                db.Plays.Add(item);
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Gemte {Count} nye afspilninger", added);
    }
}