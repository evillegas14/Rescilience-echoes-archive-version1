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
        
        // Removed the Users DbSet since authentication has been removed from the application
    }
}
