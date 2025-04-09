using Microsoft.EntityFrameworkCore;
using TravelAgent.Models.Data;

namespace TravelAgent.Data
{
    public class TravelAgentDbContext : DbContext
    {
        public TravelAgentDbContext(DbContextOptions<TravelAgentDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
