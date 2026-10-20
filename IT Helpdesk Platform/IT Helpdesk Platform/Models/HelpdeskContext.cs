using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IT_Helpdesk_Platform.Models
{
    public class HelpdeskContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public HelpdeskContext(DbContextOptions<HelpdeskContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete loops on User -> Tickets (Requester & Agent)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Requester)
                .WithMany(u => u.SubmittedTickets)
                .HasForeignKey(t => t.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}