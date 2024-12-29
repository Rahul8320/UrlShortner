using Microsoft.EntityFrameworkCore;
using UrlShortner.Domain.Model;

namespace UrlShortner.Domain.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
    public DbSet<UrlVisit> UrlVisits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasIndex(e => e.ShortCode).IsUnique();
        });

        modelBuilder.Entity<UrlVisit>()
            .HasOne(su => su.ShortenedUrl)
            .WithMany()
            .HasForeignKey(uv => uv.ShortenedUrlId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
