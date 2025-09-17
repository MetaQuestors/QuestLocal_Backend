using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Models;
using System.ComponentModel.DataAnnotations;


[Index(nameof(Email), IsUnique = true)]
public class User
{

    [Key]
    public int UserId { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? ProfileImageUrl { get; set; } = string.Empty;

    public int Coins { get; set; } = 0;
    public int Tickets { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Quest> QuestsIssued { get; set; } = new List<Quest>();
    public ICollection<UserQuest> QuestsTaken { get; set; } = new List<UserQuest>();

    // ✅ Add this:
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
