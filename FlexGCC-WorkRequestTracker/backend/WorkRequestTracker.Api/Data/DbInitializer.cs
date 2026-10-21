using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        // Apply any pending migrations; create DB if it doesn't exist
        await db.Database.EnsureCreatedAsync();

        // Enable foreign key enforcement for SQLite
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

        // Seed only if no data exists (idempotent)
        if (await db.WorkRequests.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var seed = new[]
        {
            new WorkRequest
            {
                Title = "Redesign Customer Portal",
                ClientName = "Acme Corp",
                Description = "Redesign the customer-facing portal UI with new branding guidelines and improved UX.",
                Priority = Priority.High,
                Status = WorkRequestStatus.InProgress,
                DueDate = DateOnly.FromDateTime(now.AddDays(14)),
                CreatedAtUtc = now.AddDays(-5),
                UpdatedAtUtc = now.AddDays(-2),
            },
            new WorkRequest
            {
                Title = "API Performance Audit",
                ClientName = "TechStart Inc",
                Description = "Audit all public APIs for performance bottlenecks and propose optimizations.",
                Priority = Priority.Medium,
                Status = WorkRequestStatus.New,
                DueDate = DateOnly.FromDateTime(now.AddDays(21)),
                CreatedAtUtc = now.AddDays(-3),
                UpdatedAtUtc = now.AddDays(-3),
            },
            new WorkRequest
            {
                Title = "Fix Login Bug on Mobile",
                ClientName = "RetailPlus",
                Description = "Investigate and fix the login failure reported on iOS Safari 17.",
                Priority = Priority.High,
                Status = WorkRequestStatus.Blocked,
                DueDate = DateOnly.FromDateTime(now.AddDays(3)),
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now,
            },
            new WorkRequest
            {
                Title = "Database Backup Automation",
                ClientName = "FinServ Ltd",
                Description = "Automate nightly database backup with 30-day retention policy.",
                Priority = Priority.Low,
                Status = WorkRequestStatus.Completed,
                DueDate = DateOnly.FromDateTime(now.AddDays(-7)),
                CreatedAtUtc = now.AddDays(-20),
                UpdatedAtUtc = now.AddDays(-7),
            },
            new WorkRequest
            {
                Title = "Integrate Payment Gateway",
                ClientName = "Acme Corp",
                Description = "Integrate Stripe payment gateway into the checkout flow.",
                Priority = Priority.High,
                Status = WorkRequestStatus.New,
                DueDate = DateOnly.FromDateTime(now.AddDays(30)),
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now.AddDays(-1),
            },
        };

        db.WorkRequests.AddRange(seed);
        await db.SaveChangesAsync();

        // Add notes to first request
        var notes = new[]
        {
            new WorkRequestNote
            {
                WorkRequestId = seed[0].Id,
                Text = "Initial design mockups reviewed and approved by the client.",
                CreatedAtUtc = now.AddDays(-4),
            },
            new WorkRequestNote
            {
                WorkRequestId = seed[0].Id,
                Text = "Development started. Frontend components being built.",
                CreatedAtUtc = now.AddDays(-2),
            },
            new WorkRequestNote
            {
                WorkRequestId = seed[2].Id,
                Text = "Waiting for iOS test device to reproduce the issue consistently.",
                CreatedAtUtc = now,
            },
        };

        db.WorkRequestNotes.AddRange(notes);
        await db.SaveChangesAsync();
    }
}
