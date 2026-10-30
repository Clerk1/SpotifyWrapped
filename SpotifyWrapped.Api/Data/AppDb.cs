using Microsoft.EntityFrameworkCore;
using SpotifyWrapped.Models;

namespace SpotifyWrapped.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Play> Plays => Set<Play>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<Play>().HasIndex(p => new { p.TrackId, p.PlayedAt }).IsUnique();
}