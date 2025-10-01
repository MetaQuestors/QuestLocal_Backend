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

    // ✅ Use only this for profile pictures
    public string? ProfileImageUrl { get; set; } = string.Empty;

    public int Coins { get; set; } = 0;
    public int Tickets { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Quest> QuestsIssued { get; set; } = new List<Quest>();
    public ICollection<UserQuest> QuestsTaken { get; set; } = new List<UserQuest>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public string Role { get; set; } = "User";
    public bool IsSuperAdmin()
    {
        // You can hardcode your user ID or email here
        return this.UserId == 1; // Change 1 to YOUR user ID
        // OR use email:
        // return this.Email == "your-email@domain.com";
    }

}
