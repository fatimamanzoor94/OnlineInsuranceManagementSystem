// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Models;

namespace OnlineInsuranceManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PolicyDocument> PolicyDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Application → Agent Relationship (with AssignedDate)
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Agent)
                .WithMany() // No navigation collection needed on User side
                .HasForeignKey(a => a.AgentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Message Relationships
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}