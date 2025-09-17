using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestLocalBackend.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Topic { get; set; }

        [Required]
        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User CreatedBy { get; set; }
        public List<Message> Messages { get; set; }
    }
}
