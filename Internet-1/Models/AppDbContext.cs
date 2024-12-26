using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Internet_1.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<FileModal> Files { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<FileModal> SystemFiles { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Seed();
        //}
    }
}
