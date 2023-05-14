using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamTogether.Core.Models;

[Table("TelegramPolls")]
public class TelegramPoll
{
    [Key] 
    public string PollId { get; set; } = default!;

    [Required]
    public long ChatId { get; set; }

    [Required]
    public int MessageId { get; set; }

    public ICollection<TelegramPollVote> TelegramPollVotes { get; set; } = new List<TelegramPollVote>();
}