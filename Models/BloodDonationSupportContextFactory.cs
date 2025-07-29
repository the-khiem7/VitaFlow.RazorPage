using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Models
{
    public class BloodDonationSupportContextFactory : IDesignTimeDbContextFactory<BloodDonationSupportContext>
    {
        public BloodDonationSupportContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BloodDonationSupportContext>();
            
            // Get the connection string from appsettings.json in the RazorPage project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new BloodDonationSupportContext(optionsBuilder.Options);
        }
    }
} 