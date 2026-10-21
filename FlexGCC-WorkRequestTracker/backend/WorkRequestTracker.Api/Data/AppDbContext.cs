using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WorkRequest> WorkRequests { get; set; }
    public DbSet<WorkRequestNote> WorkRequestNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DueDate).HasConversion(
                v => v.ToString("yyyy-MM-dd"),
                v => DateOnly.Parse(v));

            entity.HasIndex(e => e.Status).HasDatabaseName("IX_WorkRequests_Status");
            entity.HasIndex(e => e.DueDate).HasDatabaseName("IX_WorkRequests_DueDate");
            entity.HasIndex(e => e.CreatedAtUtc).HasDatabaseName("IX_WorkRequests_CreatedAtUtc");

            entity.HasMany(e => e.Notes)
                  .WithOne(n => n.WorkRequest)
                  .HasForeignKey(n => n.WorkRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkRequestNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
        });
    }
}
