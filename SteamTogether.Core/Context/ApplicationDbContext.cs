using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Models;

namespace SteamTogether.Core.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<SteamPlayer> SteamPlayers { get; set; } = default!;
    public DbSet<TelegramChat> TelegramChat { get; set; } = default!;
    public DbSet<SteamGame> SteamGames { get; set; } = default!;

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
}
