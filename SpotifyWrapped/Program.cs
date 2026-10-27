namespace SpotifyWrapped;

using SpotifyWrapped.Models;
using SpotifyWrapped.Services;
using Microsoft.EntityFrameworkCore;
using SpotifyWrapped.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls("http://127.0.0.1:5000");

        builder.Services.AddControllersWithViews();
        builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<TokenStore>();
        builder.Services.AddSingleton<SpotifyAuth>();
        builder.Services.AddSingleton<SpotifyClient>();
        builder.Services.AddDbContext<AppDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddHostedService<PollingService>();

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}