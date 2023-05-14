using Microsoft.EntityFrameworkCore;
using SteamTogether.Core.Models;

namespace SteamTogether.Core.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<SteamPlayer> SteamPlayers { get; set; } = default!;
    public DbSet<TelegramChatParticipant> TelegramChatParticipants { get; set; } = default!;
    public DbSet<SteamGame> SteamGames { get; set; } = default!;
    public DbSet<SteamGameCategory> SteamGamesCategories { get; set; } = default!;
    public DbSet<TelegramPoll> TelegramPolls { get; set; } = default!;
    public DbSet<TelegramPollVote> TelegramPollVotes { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SteamPlayer>().HasKey(player => player.PlayerId);
        
        modelBuilder.Entity<TelegramPoll>()
            .HasIndex(poll => new {poll.ChatId})
            .IsUnique();
        
        modelBuilder.Entity<TelegramPollVote>()
            .HasIndex(vote => new {vote.PollId, vote.TelegramUserId})
            .IsUnique();

        modelBuilder.Entity<TelegramPollVote>()
            .HasIndex(vote => new {vote.PollId});

        modelBuilder.Entity<TelegramPoll>()
            .HasMany(p => p.TelegramPollVotes)
            .WithOne(p => p.TelegramPoll)
            .HasForeignKey(pv => pv.PollId)
            .OnDelete(DeleteBehavior.Cascade);

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
        
        modelBuilder
            .Entity<SteamGame>()
            .HasMany<SteamGameCategory>(g => g.Categories)
            .WithMany(c => c.Games)
            .UsingEntity<Dictionary<string, object>>(
                "SteamGameSteamCategory",
                r => r.HasOne<SteamGameCategory>().WithMany().HasForeignKey("CategoryId"),
                l => l.HasOne<SteamGame>().WithMany().HasForeignKey("GameId"),
                je =>
                {
                    je.HasKey("GameId", "CategoryId");
                }
            );

        modelBuilder
            .Entity<TelegramChatParticipant>()
            .HasOne<SteamPlayer>(c => c.SteamPlayer);

        modelBuilder
            .Entity<TelegramChatParticipant>()
            .HasKey(c => new {c.ChatId, c.TelegramUserId, c.SteamPlayerId});
            
        modelBuilder
            .Entity<TelegramChatParticipant>()
            .HasIndex(c => new {c.ChatId, c.TelegramUserId});
        
        modelBuilder
            .Entity<TelegramChatParticipant>()
            .HasIndex(c => new {c.ChatId, c.SteamPlayerId});
    }
}
