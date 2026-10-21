using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.DTOs;

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
    public IEnumerable<WorkRequestNoteResponseDto> Notes { get; set; } = new List<WorkRequestNoteResponseDto>();
}

public class WorkRequestListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class WorkRequestNoteResponseDto
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
