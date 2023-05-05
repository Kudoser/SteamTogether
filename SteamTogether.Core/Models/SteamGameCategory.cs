using System.ComponentModel.DataAnnotations;

namespace SteamTogether.Core.Models;

public class SteamGameCategory
{
    [Key]
    public uint CategoryId { get; set; }
    public string? Description { get; set; }
    
    public ICollection<SteamGame> Games { get; } = new List<SteamGame>();
}
