using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.DTOs;
using Xunit;

namespace WorkRequestTracker.Tests;

/// <summary>
/// Integration tests using WebApplicationFactory with an isolated in-memory SQLite database.
/// Each test class instance gets its own isolated database.
/// </summary>
public class WorkRequestApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WorkRequestApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Helper: create a valid request and return parsed response
    private async Task<WorkRequestResponseDto> CreateValidRequestAsync(
        string title = "Test Request",
        string clientName = "Test Client",
        string description = "Test description body",
        string priority = "Medium",
        string status = "New",
        string dueDate = "2027-01-01")
    {
        var payload = new
        {
            title,
            clientName,
            description,
            priority,
            status,
            dueDate,
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;
    }

    // ── 1. Create a valid request ──────────────────────────────────────────
    [Fact]
    public async Task CreateRequest_ValidPayload_Returns201WithDto()
    {
        var payload = new
        {
            title = "New Task",
            clientName = "Acme Corp",
            description = "Detailed description here.",
            priority = "High",
            status = "New",
            dueDate = "2027-06-30",
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;
        Assert.True(dto.Id > 0);
        Assert.Equal("New Task", dto.Title);
        Assert.Equal("Acme Corp", dto.ClientName);
        Assert.Equal("High", dto.Priority);
        Assert.Equal("New", dto.Status);
        Assert.Equal("2027-06-30", dto.DueDate);
    }

    // ── 2. Reject missing title ────────────────────────────────────────────
    [Fact]
    public async Task CreateRequest_MissingTitle_Returns400()
    {
        var payload = new
        {
            clientName = "Client",
            description = "Description",
            priority = "Low",
            status = "New",
            dueDate = "2027-01-01",
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 2b. Reject past due date ───────────────────────────────────────────
    [Fact]
    public async Task CreateRequest_PastDueDate_Returns400()
    {
        var payload = new
        {
            title = "Past Task",
            clientName = "Client",
            description = "Description",
            priority = "Low",
            status = "New",
            dueDate = "2020-01-01", // Past date
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 3. Reject missing client name ──────────────────────────────────────
    [Fact]
    public async Task CreateRequest_MissingClientName_Returns400()
    {
        var payload = new
        {
            title = "Title",
            description = "Description",
            priority = "Low",
            status = "New",
            dueDate = "2027-01-01",
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 4. Reject invalid priority ─────────────────────────────────────────
    [Fact]
    public async Task CreateRequest_InvalidPriority_Returns400()
    {
        var payload = new
        {
            title = "Title",
            clientName = "Client",
            description = "Description",
            priority = "Critical",   // invalid
            status = "New",
            dueDate = "2027-01-01",
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 5. Reject invalid status on create ────────────────────────────────
    [Fact]
    public async Task CreateRequest_InvalidStatus_Returns400()
    {
        var payload = new
        {
            title = "Title",
            clientName = "Client",
            description = "Description",
            priority = "Low",
            status = "Pending",   // invalid
            dueDate = "2027-01-01",
        };

        var response = await _client.PostAsJsonAsync("/api/work-requests", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 6. Return 404 for unknown ID ──────────────────────────────────────
    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync("/api/work-requests/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── 7. Update status successfully ─────────────────────────────────────
    [Fact]
    public async Task UpdateStatus_ValidStatus_Returns200WithUpdatedStatus()
    {
        var created = await CreateValidRequestAsync(status: "New");

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/work-requests/{created.Id}/status",
            new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        var body = await patchResponse.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;
        Assert.Equal("InProgress", dto.Status);
    }

    // ── 8. Persist a newly added note ─────────────────────────────────────
    [Fact]
    public async Task AddNote_ValidText_Returns201AndNoteIsRetrievable()
    {
        var created = await CreateValidRequestAsync();

        var noteResponse = await _client.PostAsJsonAsync(
            $"/api/work-requests/{created.Id}/notes",
            new { text = "This is a note." });

        Assert.Equal(HttpStatusCode.Created, noteResponse.StatusCode);

        // Verify note appears in GetById
        var getResponse = await _client.GetAsync($"/api/work-requests/{created.Id}");
        var body = await getResponse.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;
        Assert.Single(dto.Notes);
        Assert.Equal("This is a note.", dto.Notes.First().Text);
    }

    // ── 9. Reject empty notes ─────────────────────────────────────────────
    [Fact]
    public async Task AddNote_EmptyText_Returns400()
    {
        var created = await CreateValidRequestAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/work-requests/{created.Id}/notes",
            new { text = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 9b. Reject whitespace-only notes ──────────────────────────────────
    [Fact]
    public async Task AddNote_WhitespaceText_Returns400()
    {
        var created = await CreateValidRequestAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/work-requests/{created.Id}/notes",
            new { text = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 10. Filter by status ──────────────────────────────────────────────
    [Fact]
    public async Task GetList_FilterByStatus_ReturnsOnlyMatchingItems()
    {
        await CreateValidRequestAsync(status: "New");
        await CreateValidRequestAsync(status: "InProgress");
        await CreateValidRequestAsync(status: "New");

        var response = await _client.GetAsync("/api/work-requests?status=New&pageSize=100");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultHelper>(body, JsonOpts)!;

        Assert.All(result.Items, item => Assert.Equal("New", item.Status));
    }

    // ── 11. Search by title ───────────────────────────────────────────────
    [Fact]
    public async Task GetList_SearchByTitle_ReturnsMatchingItems()
    {
        await CreateValidRequestAsync(title: "Unique XYZ Title 123");
        await CreateValidRequestAsync(title: "Other Task");

        var response = await _client.GetAsync("/api/work-requests?search=XYZ&pageSize=100");
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultHelper>(body, JsonOpts)!;

        Assert.All(result.Items, item => Assert.Contains("XYZ", item.Title, StringComparison.OrdinalIgnoreCase));
    }

    // ── 12. Search by client name ─────────────────────────────────────────
    [Fact]
    public async Task GetList_SearchByClientName_ReturnsMatchingItems()
    {
        await CreateValidRequestAsync(clientName: "UniqueClientABC");
        await CreateValidRequestAsync(clientName: "SomeOtherClient");

        var response = await _client.GetAsync("/api/work-requests?search=UniqueClientABC&pageSize=100");
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultHelper>(body, JsonOpts)!;

        Assert.All(result.Items, item =>
            Assert.Contains("UniqueClientABC", item.ClientName, StringComparison.OrdinalIgnoreCase));
    }

    // ── 13. Correct pagination metadata ──────────────────────────────────
    [Fact]
    public async Task GetList_Pagination_MetadataIsCorrect()
    {
        // Use a fresh factory scope so we control exact count
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existingCount = await db.WorkRequests.CountAsync();

        // Create 5 more
        for (int i = 0; i < 5; i++)
            await CreateValidRequestAsync(title: $"PaginationTest_{i}_{Guid.NewGuid()}");

        var response = await _client.GetAsync("/api/work-requests?page=1&pageSize=2");
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedMetaHelper>(body, JsonOpts)!;

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count());
        Assert.True(result.TotalCount >= 5);
        Assert.True(result.TotalPages >= 3);
    }

    // ── 14. Reject invalid page and page-size values ─────────────────────
    [Fact]
    public async Task GetList_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/api/work-requests?page=0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetList_InvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/api/work-requests?pageSize=0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetList_PageSizeTooLarge_Returns400()
    {
        var response = await _client.GetAsync("/api/work-requests?pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 15. Return request with its notes ─────────────────────────────────
    [Fact]
    public async Task GetById_RequestWithNotes_ReturnsNotesInResponse()
    {
        var created = await CreateValidRequestAsync();
        await _client.PostAsJsonAsync($"/api/work-requests/{created.Id}/notes", new { text = "Note one" });
        await _client.PostAsJsonAsync($"/api/work-requests/{created.Id}/notes", new { text = "Note two" });

        var response = await _client.GetAsync($"/api/work-requests/{created.Id}");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;

        Assert.Equal(2, dto.Notes.Count());
    }

    // ── 16. Timestamps update after mutations ──────────────────────────────
    [Fact]
    public async Task UpdateStatus_UpdatesUpdatedAtUtc()
    {
        var created = await CreateValidRequestAsync();
        var originalUpdatedAt = created.UpdatedAtUtc;

        // Small delay to ensure timestamp differs
        await Task.Delay(50);

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/work-requests/{created.Id}/status",
            new { status = "Completed" });
        patchResponse.EnsureSuccessStatusCode();

        var body = await patchResponse.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;

        Assert.True(dto.UpdatedAtUtc >= originalUpdatedAt);
    }

    [Fact]
    public async Task AddNote_UpdatesParentUpdatedAtUtc()
    {
        var created = await CreateValidRequestAsync();
        await Task.Delay(50);

        await _client.PostAsJsonAsync(
            $"/api/work-requests/{created.Id}/notes",
            new { text = "Checking timestamp." });

        var getResponse = await _client.GetAsync($"/api/work-requests/{created.Id}");
        var body = await getResponse.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<WorkRequestResponseDto>(body, JsonOpts)!;

        Assert.True(dto.UpdatedAtUtc >= created.UpdatedAtUtc);
    }

    // ── 17. UpdateStatus 404 for unknown ID ───────────────────────────────
    [Fact]
    public async Task UpdateStatus_UnknownId_Returns404()
    {
        var response = await _client.PatchAsJsonAsync(
            "/api/work-requests/999999/status",
            new { status = "New" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── 18. AddNote 404 for unknown ID ────────────────────────────────────
    [Fact]
    public async Task AddNote_UnknownId_Returns404()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/work-requests/999999/notes",
            new { text = "Some note" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── 19. Reject invalid status string on PATCH ─────────────────────────
    [Fact]
    public async Task UpdateStatus_InvalidStatus_Returns400()
    {
        var created = await CreateValidRequestAsync();

        var response = await _client.PatchAsJsonAsync(
            $"/api/work-requests/{created.Id}/status",
            new { status = "NotAStatus" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── 20. Filter returns empty collection for no matches ─────────────────
    [Fact]
    public async Task GetList_FilterWithNoMatches_ReturnsEmptyCollection()
    {
        var response = await _client.GetAsync("/api/work-requests?search=ZZZNOMATCH12345XYZ");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedMetaHelper>(body, JsonOpts)!;
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}

// Helper classes for deserialization
public class WorkRequestResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IEnumerable<NoteDto> Notes { get; set; } = new List<NoteDto>();
}

public class NoteDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class ListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PagedResultHelper
{
    public IEnumerable<ListItemDto> Items { get; set; } = new List<ListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PagedMetaHelper
{
    public IEnumerable<ListItemDto> Items { get; set; } = new List<ListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
