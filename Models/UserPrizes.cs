namespace QuestLocalBackend.Models
{
    public class UserPrize
    {
        public int UserPrizeId { get; set; }
        public int UserId { get; set; }
        public int PrizeId { get; set; }
        public DateTime RedeemedAt { get; set; }

        public User User { get; set; }
        public Prize Prize { get; set; }
    }

}
