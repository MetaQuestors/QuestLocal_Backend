using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Quest
{
    [Key]
    public int QuestId { get; set; }

    [Required]
    public int IssuerId { get; set; }

    [ForeignKey("IssuerId")]
    public User Issuer { get; set; } = null!;

    [Required]
    public string Heading { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CoinReward { get; set; }

    public int XPReward { get; set; }

    public string? BadgeReward { get; set; }

    public string? Location { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? EstimatedTime { get; set; }

    public string? QuestType { get; set; }

    public string? Goal { get; set; }

    [Required]
    public string VerificationType { get; set; } = "None";

    public bool IsActive { get; set; } = true;

    public bool IsPrivate { get; set; } = false;

    public string ActivationTime { get; set; } = "Now";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime DueDate { get; set; }

    // Add this property
    public string ScreenshotUrl { get; set; } = string.Empty; // Screenshot URL property

    public ICollection<UserQuest> UsersTaken { get; set; } = new List<UserQuest>();
}
