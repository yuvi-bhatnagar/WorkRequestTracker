using Microsoft.AspNetCore.Mvc;
using WorkRequestTracker.Api.DTOs;
using WorkRequestTracker.Api.Models;
using WorkRequestTracker.Api.Services;

namespace WorkRequestTracker.Api.Controllers;

[ApiController]
[Route("api/work-requests")]
[Produces("application/json")]
public class WorkRequestsController : ControllerBase
{
    private readonly IWorkRequestService _service;

    public WorkRequestsController(IWorkRequestService service)
    {
        _service = service;
    }

    /// <summary>List, filter, search, and paginate work requests.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WorkRequestListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            return BadRequest(ProblemDetailsFor("Page must be 1 or greater."));

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(ProblemDetailsFor("PageSize must be between 1 and 100."));

        WorkRequestStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<WorkRequestStatus>(status.Trim(), ignoreCase: false, out var s))
                return BadRequest(ProblemDetailsFor(
                    $"Invalid status value '{status}'. Allowed: New, InProgress, Blocked, Completed."));
            parsedStatus = s;
        }

        var result = await _service.GetListAsync(parsedStatus, search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a specific work request by ID, including notes.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null)
            return NotFound(ProblemDetailsFor404(id));
        return Ok(result);
    }

    /// <summary>Create a new work request.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        // Reject whitespace-only strings after trimming
        if (string.IsNullOrWhiteSpace(dto.Title))
            ModelState.AddModelError(nameof(dto.Title), "Title cannot be whitespace only.");
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            ModelState.AddModelError(nameof(dto.ClientName), "ClientName cannot be whitespace only.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            ModelState.AddModelError(nameof(dto.Description), "Description cannot be whitespace only.");

        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update the status of a work request.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(WorkRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateWorkRequestStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        var result = await _service.UpdateStatusAsync(id, dto.Status!.Value);
        if (result is null)
            return NotFound(ProblemDetailsFor404(id));

        return Ok(result);
    }

    /// <summary>Add a note to a work request.</summary>
    [HttpPost("{id:int}/notes")]
    [ProducesResponseType(typeof(WorkRequestNoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(int id, [FromBody] AddWorkRequestNoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ValidationProblemDetails(ModelState));

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            ModelState.AddModelError(nameof(dto.Text), "Text cannot be whitespace only.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var result = await _service.AddNoteAsync(id, dto.Text);
        if (result is null)
            return NotFound(ProblemDetailsFor404(id));

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    private ProblemDetails ProblemDetailsFor(string detail) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Bad Request",
        Detail = detail,
        Instance = HttpContext.Request.Path,
    };

    private ProblemDetails ProblemDetailsFor404(int id) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Not Found",
        Detail = $"Work request with id {id} was not found.",
        Instance = HttpContext.Request.Path,
    };
}
