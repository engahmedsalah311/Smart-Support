using Microsoft.EntityFrameworkCore;
using SmartSupport.Api.AI.Documents.Entities;

namespace SmartSupport.Api.Data
{
    public class SmartSupportDbContext:DbContext
    {
        public SmartSupportDbContext(DbContextOptions<SmartSupportDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Document> Documents { get; set; }
    }
}
