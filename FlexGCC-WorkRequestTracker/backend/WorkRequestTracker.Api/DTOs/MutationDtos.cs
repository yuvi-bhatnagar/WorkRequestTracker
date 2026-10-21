using System.ComponentModel.DataAnnotations;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.DTOs;

public class UpdateWorkRequestStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public WorkRequestStatus? Status { get; set; }
}

public class AddWorkRequestNoteDto
{
    [Required(ErrorMessage = "Text is required.")]
    [MaxLength(2000, ErrorMessage = "Text must not exceed 2000 characters.")]
    public string Text { get; set; } = string.Empty;
}
