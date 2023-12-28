using AI_Chess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;

namespace AI_Chess.Context
{

    public class ChessDbContext : DbContext
    {
        public DbSet<WContent> WContents { get; set; }
        public DbSet<BContent> BContents { get; set; }

        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BContent>()
                .HasKey(bc => new { bc.Position, bc.To });

            modelBuilder.Entity<WContent>()
                .HasKey(bc => new { bc.Position, bc.To, bc.From });
        }

    }
}
