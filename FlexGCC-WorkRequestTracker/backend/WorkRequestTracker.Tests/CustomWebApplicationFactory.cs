using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkRequestTracker.Api.Data;

namespace WorkRequestTracker.Tests;

/// <summary>
/// Test factory that replaces the real SQLite database with an isolated
/// temp-file SQLite database. Each factory instance gets its own unique
/// file so test runs are completely isolated from the app database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dbPath;

    public CustomWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"wrt_test_{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Pass db path via env so Program.cs picks it up
        builder.UseSetting("Database:Path", _dbPath);
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove any registered DbContextOptions<AppDbContext>
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            // Register isolated file-based SQLite
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Clean up temp db file after tests
        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { /* ignore cleanup errors */ }
        }
    }
}
