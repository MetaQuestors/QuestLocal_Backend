namespace QuestLocalBackend.Models
{
    public class CreateQuestRequest
    {
        public int IssuerId { get; set; }
        public string Heading { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CoinReward { get; set; }
        public string VerificationType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int XPReward { get; set; }
        public string BadgeReward { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public string QuestType { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public string ActivationTime { get; set; } = "Now"; // Changed to string
        public string ScreenshotUrl { get; set; } = string.Empty; // New field for the assignment screenshot
    }

}
