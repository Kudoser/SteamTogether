using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteamTogether.Bot.Models;
using SteamTogether.Bot.Options;

namespace SteamTogether.Bot.Context;

public class ApplicationDbContext : DbContext
{
    private readonly DatabaseOptions _options;
    public DbSet<SteamPlayer> SteamPlayers { get; set; }
    public DbSet<TelegramChat> TelegramChat { get; set; }

    public ApplicationDbContext(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SteamPlayer>()
            .HasKey(player => player.PlayerId);

        modelBuilder
            .Entity<SteamPlayer>()
            .HasData(_options.SeedData.SteamPlayers);

        modelBuilder
            .Entity<TelegramChat>()
            .HasKey(chat => chat.ChatId);

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
                    je.HasData(
                        _options.SeedData.SteamPlayers.Select(player => new
                        {
                            PlayerId = player.PlayerId,
                            ChatId = (long)1
                        })
                    );
                });
        
        modelBuilder
            .Entity<TelegramChat>()
            .HasData(new TelegramChat {ChatId = 1});
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.ConnectionString);

        optionsBuilder.UseSqlite(_options.ConnectionString);
    }
}