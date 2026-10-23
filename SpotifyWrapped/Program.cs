namespace SpotifyWrapped;

using SpotifyWrapped.Models;
using SpotifyWrapped.Services;

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

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}