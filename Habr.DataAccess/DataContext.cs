using Habr.DataAccess.Configurations;
using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Habr.DataAccess
{
    public class DataContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostConfiguration).Assembly);
        }
    }
}
