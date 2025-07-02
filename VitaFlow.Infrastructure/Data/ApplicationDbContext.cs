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
        public DbSet<YourEntity> YourEntities { get; set; } // Replace with actual entity

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
