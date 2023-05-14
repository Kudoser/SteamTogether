using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("TelegramPollVotes")]
public class TelegramPollVote
{
    [Key]
    public int PollVoteId { get; set; }

    [Required]
    public string PollId { get; set; } = default!;
    
    [Required]
    public long TelegramUserId { get; set; }

    public TelegramPoll TelegramPoll { get; set; } = default!;
}