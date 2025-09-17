using QuestLocalBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class UserQuest
{
    [Key]
    public int UserQuestId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [ForeignKey("Quest")]
    public int QuestId { get; set; }

    public Quest Quest { get; set; } = null!;

    public DateTime AcceptedAt { get; set; } = DateTime.Now;

    public bool IsCompleted { get; set; } = false;

    public bool IsVerified { get; set; } = false;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "InProgress";
}
