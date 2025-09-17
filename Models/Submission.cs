using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestLocalBackend.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        [ForeignKey("UserQuest")]
        public int UserQuestId { get; set; }

        [Required]
        public string FileUrl { get; set; } // Non-nullable, initialized in constructor or via required attribute

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public UserQuest UserQuest { get; set; }
    }
}
