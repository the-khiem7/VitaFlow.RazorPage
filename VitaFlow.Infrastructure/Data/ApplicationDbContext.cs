using Microsoft.EntityFrameworkCore;
using VitaFlow.Core.Entities;

namespace VitaFlow.Infrastructure.Data
{
    /// <summary>
    /// Represents the database context for the VitaFlow application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for entities
        public DbSet<User> Users { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<BloodDonation> BloodDonations { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<BloodInventory> BloodInventories { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Configures the entity relationships and other settings.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships here
        }
    }
}
