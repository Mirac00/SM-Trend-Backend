using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api.Helpers
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlServer(Configuration.GetConnectionString("ApiDatabase"));
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostFile> PostFiles { get; set; }
        public DbSet<PostLikeDislike> PostLikeDislikes { get; set; } // Dodane pole

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().Property(p => p.Likes).HasDefaultValue(0);
            modelBuilder.Entity<Post>().Property(p => p.Dislikes).HasDefaultValue(0);
        }
    }
}
