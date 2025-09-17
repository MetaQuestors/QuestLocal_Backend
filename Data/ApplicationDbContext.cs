using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Models;

namespace QuestLocalBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<UserQuest> UserQuests { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Prize> Prizes { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserPrize> UserPrizes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserQuest>()
                .HasOne(uq => uq.User)
                .WithMany(u => u.QuestsTaken)
                .HasForeignKey(uq => uq.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<UserQuest>()
                .HasOne(uq => uq.Quest)
                .WithMany(q => q.UsersTaken)
                .HasForeignKey(uq => uq.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<UserSettings>()
                .HasOne(us => us.User)
                .WithOne()
                .HasForeignKey<UserSettings>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
        }


    }
}