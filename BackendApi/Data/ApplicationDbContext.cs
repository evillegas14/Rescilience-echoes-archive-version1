using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Represents the Posts table in the database
        public DbSet<Post> Posts { get; set; }

        // Represents the Users table in the database
        public DbSet<User> Users { get; set; }
    }
}
