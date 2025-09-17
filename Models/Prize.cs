using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;

    namespace QuestLocalBackend.Models
    {
        public class Prize
        {
            [Key]
            public int PrizeId { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }

            [Required]
            public int TicketCost { get; set; }

            public bool IsAvailable { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            // ✅ New field to support prize image
            [StringLength(300)]
            public string ImageUrl { get; set; }
        }
    }


