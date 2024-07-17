using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDatabase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Documents> Documents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Documents>()
                .HasKey(d => d.DocumentId); // Ensure the primary key is set

            base.OnModelCreating(modelBuilder);
        }
    }
}