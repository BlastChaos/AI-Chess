using AI_Chess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
namespace AI_Chess.Context
{

    public class ChessDbContext : DbContext
    {
        public DbSet<WContent> WContents { get; set; }
        public DbSet<ZContent> ZContents { get; set; }
        public DbSet<AContent> AContents { get; set; }
        public DbSet<BContent> BContents { get; set; }

        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BContent>()
            .Property(e => e.Value)
                .HasConversion(
                    v => ConvertArrayDoubleToString(v),
                    v => ConvertToArrayDouble2(v))
            .Metadata.SetValueComparer(new ValueComparer<double[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray()));


            modelBuilder.Entity<BContent>()
                .HasIndex(e => e.Position)
                .IsUnique();

            modelBuilder.Entity<WContent>()
                .Property(e => e.Value)
                .HasConversion(
                    v => ConvertArrayDoubleToString(v),
                    v => ConvertToArrayDouble(v))
            .Metadata.SetValueComparer(new ValueComparer<double[][]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray()));

            modelBuilder.Entity<WContent>()
                .HasIndex(e => e.Position)
                .IsUnique();

            modelBuilder.Entity<ZContent>()
                .Property(e => e.Value)
                .HasConversion(
                    v => ConvertArrayDoubleToString(v),
                    v => ConvertToArrayDouble(v))
            .Metadata.SetValueComparer(new ValueComparer<double[][]>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray()));

            modelBuilder.Entity<ZContent>()
                .HasIndex(e => e.Position)
                .IsUnique();

            modelBuilder.Entity<AContent>()
                .Property(e => e.Value)
                .HasConversion(
                    v => ConvertArrayDoubleToString(v),
                    v => ConvertToArrayDouble(v))
            .Metadata.SetValueComparer(new ValueComparer<double[][]>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray()));

            modelBuilder.Entity<AContent>()
                .HasIndex(e => e.Position)
                .IsUnique();
        }

        private double[][] ConvertToArrayDouble(string v){
            return JsonSerializer.Deserialize<double[][]>(v) ?? Array.Empty<double[]>();
        }

        private string ConvertArrayDoubleToString(double[][] v){
            return JsonSerializer.Serialize(v);
        }

        private double[] ConvertToArrayDouble2(string v){
            return JsonSerializer.Deserialize<double[]>(v) ?? Array.Empty<double>();
        }

        private string ConvertArrayDoubleToString(double[] v){
            return JsonSerializer.Serialize(v);
        }

    }
}
