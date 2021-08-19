using Microsoft.EntityFrameworkCore;
using TheGame.Common.Models;

namespace TheGame.DataService
{
    /// <summary>
    /// Entity Framework Code First Approach
    /// </summary>
    public class TheGameDatabaseContext : DbContext
    {
        /// <summary>
        /// The DbSet properties tells EF Core what tables are needed be created
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// Constructor - the connection string is injected wihtin options property
        /// </summary>
        /// <param name="options"></param>
        public TheGameDatabaseContext(DbContextOptions<TheGameDatabaseContext> options) : base(options)
        {
        }

        /// <summary>
        ///  OnModelCreating function provides us the ability to manage the tables properties
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}