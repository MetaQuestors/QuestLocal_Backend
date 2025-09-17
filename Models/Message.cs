using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestLocalBackend.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        [ForeignKey("User")]
        public int UserId { get; set; }

        [InverseProperty("Messages")]
        public User User { get; set; } = null!;
    }
}