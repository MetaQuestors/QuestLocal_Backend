using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestLocalBackend.Models
{
    public class UserSettings
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public bool IsDarkMode { get; set; } = false;
        public bool NotificationsEnabled { get; set; } = true;
        public string Language { get; set; } = "English";
        public bool AutoSave { get; set; } = true;

        public User User { get; set; } = null!;
    }
}
