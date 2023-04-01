using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteamTogether.Bot.Models;
using SteamTogether.Bot.Options;

namespace SteamTogether.Bot.Context;

public class ApplicationDbContext : DbContext
{
    private readonly DatabaseOptions _options;
    public DbSet<SteamPlayer> Players { get; set; }

    public ApplicationDbContext(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SteamPlayer>()
            .HasData(_options.SeedData.SteamPlayers);
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.ConnectionString);

        optionsBuilder.UseSqlite(_options.ConnectionString);
    }
}