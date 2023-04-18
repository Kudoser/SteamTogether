using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteamTogether.Core.Models;
using SteamTogether.Core.Options;

namespace SteamTogether.Core.Context;

public class ApplicationDbContext : DbContext
{
    private readonly DatabaseOptions _options;
    public DbSet<SteamPlayer> SteamPlayers { get; set; } = default!;
    public DbSet<TelegramChat> TelegramChat { get; set; } = default!;
    public DbSet<SteamGame> SteamGames { get; set; } = default!;

    public ApplicationDbContext(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SteamPlayer>().HasKey(player => player.PlayerId);
        modelBuilder
            .Entity<SteamPlayer>()
            .HasMany<SteamGame>(p => p.Games)
            .WithMany(g => g.Players)
            .UsingEntity<Dictionary<string, object>>(
                "SteamPlayerSteamGame",
                r => r.HasOne<SteamGame>().WithMany().HasForeignKey("GameId"),
                l => l.HasOne<SteamPlayer>().WithMany().HasForeignKey("PlayerId"),
                je =>
                {
                    je.HasKey("PlayerId", "GameId");
                }
            );

        modelBuilder.Entity<TelegramChat>().HasKey(chat => chat.ChatId);

        modelBuilder
            .Entity<TelegramChat>()
            .HasMany(e => e.Players)
            .WithMany(e => e.TelegramChats)
            .UsingEntity<Dictionary<string, object>>(
                "SteamPlayerTelegramChat",
                r => r.HasOne<SteamPlayer>().WithMany().HasForeignKey("PlayerId"),
                l => l.HasOne<TelegramChat>().WithMany().HasForeignKey("ChatId"),
                je =>
                {
                    je.HasKey("PlayerId", "ChatId");
                }
            );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.ConnectionString);
        
        optionsBuilder.UseSqlite(_options.ConnectionString);
    }
}
