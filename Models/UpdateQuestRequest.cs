namespace QuestLocalBackend.Models
{
    public class UpdateQuestRequest
    {
        public int IssuerId { get; set; }
        public string? Heading { get; set; }
        public string? Description { get; set; }
        public int? CoinReward { get; set; }
        public string? VerificationType { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? IsActive { get; set; }
    }

}
