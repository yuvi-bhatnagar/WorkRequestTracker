using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.DTOs;
using WorkRequestTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace WorkRequestTracker.Api.Services;

public interface IWorkRequestService
{
    Task<PagedResult<WorkRequestListItemDto>> GetListAsync(
        WorkRequestStatus? status,
        string? search,
        int page,
        int pageSize);

    Task<WorkRequestResponseDto?> GetByIdAsync(int id);
    Task<WorkRequestResponseDto> CreateAsync(CreateWorkRequestDto dto);
    Task<WorkRequestResponseDto?> UpdateStatusAsync(int id, WorkRequestStatus status);
    Task<WorkRequestNoteResponseDto?> AddNoteAsync(int id, string text);
}

public class WorkRequestService : IWorkRequestService
{
    private readonly AppDbContext _db;

    public WorkRequestService(AppDbContext db)
    {
        _db = db;
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static WorkRequestListItemDto ToListItem(WorkRequest wr) => new()
    {
        Id = wr.Id,
        Title = wr.Title,
        ClientName = wr.ClientName,
        Priority = wr.Priority.ToString(),
        Status = wr.Status.ToString(),
        DueDate = wr.DueDate.ToString("yyyy-MM-dd"),
        CreatedAtUtc = EnsureUtc(wr.CreatedAtUtc),
        UpdatedAtUtc = EnsureUtc(wr.UpdatedAtUtc),
    };

    private static WorkRequestResponseDto ToResponse(WorkRequest wr) => new()
    {
        Id = wr.Id,
        Title = wr.Title,
        ClientName = wr.ClientName,
        Description = wr.Description,
        Priority = wr.Priority.ToString(),
        Status = wr.Status.ToString(),
        DueDate = wr.DueDate.ToString("yyyy-MM-dd"),
        CreatedAtUtc = EnsureUtc(wr.CreatedAtUtc),
        UpdatedAtUtc = EnsureUtc(wr.UpdatedAtUtc),
        Notes = wr.Notes.OrderBy(n => n.CreatedAtUtc).Select(n => new WorkRequestNoteResponseDto
        {
            Id = n.Id,
            WorkRequestId = n.WorkRequestId,
            Text = n.Text,
            CreatedAtUtc = EnsureUtc(n.CreatedAtUtc),
        }),
    };

    public async Task<PagedResult<WorkRequestListItemDto>> GetListAsync(
        WorkRequestStatus? status,
        string? search,
        int page,
        int pageSize)
    {
        var query = _db.WorkRequests.AsNoTracking();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(s) ||
                r.ClientName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WorkRequestListItemDto>
        {
            Items = items.Select(ToListItem),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<WorkRequestResponseDto?> GetByIdAsync(int id)
    {
        var wr = await _db.WorkRequests
            .AsNoTracking()
            .Include(r => r.Notes)
            .FirstOrDefaultAsync(r => r.Id == id);

        return wr is null ? null : ToResponse(wr);
    }

    public async Task<WorkRequestResponseDto> CreateAsync(CreateWorkRequestDto dto)
    {
        var now = DateTime.UtcNow;
        var wr = new WorkRequest
        {
            Title = dto.Title.Trim(),
            ClientName = dto.ClientName.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority!.Value,
            Status = dto.Status!.Value,
            DueDate = dto.DueDate!.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        _db.WorkRequests.Add(wr);
        await _db.SaveChangesAsync();

        return ToResponse(wr);
    }

    public async Task<WorkRequestResponseDto?> UpdateStatusAsync(int id, WorkRequestStatus status)
    {
        var wr = await _db.WorkRequests.FindAsync(id);
        if (wr is null) return null;

        wr.Status = status;
        wr.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<WorkRequestNoteResponseDto?> AddNoteAsync(int id, string text)
    {
        var wr = await _db.WorkRequests.FindAsync(id);
        if (wr is null) return null;

        var now = DateTime.UtcNow;
        var note = new WorkRequestNote
        {
            WorkRequestId = id,
            Text = text.Trim(),
            CreatedAtUtc = now,
        };

        _db.WorkRequestNotes.Add(note);
        wr.UpdatedAtUtc = now;
        await _db.SaveChangesAsync();

        return new WorkRequestNoteResponseDto
        {
            Id = note.Id,
            WorkRequestId = note.WorkRequestId,
            Text = note.Text,
            CreatedAtUtc = EnsureUtc(note.CreatedAtUtc),
        };
    }
}
