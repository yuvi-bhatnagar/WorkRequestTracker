using System.ComponentModel.DataAnnotations;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.DTOs;

public class CreateWorkRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(150, ErrorMessage = "Title must not exceed 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClientName is required.")]
    [MaxLength(150, ErrorMessage = "ClientName must not exceed 150 characters.")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required.")]
    public Priority? Priority { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public WorkRequestStatus? Status { get; set; }

    [Required(ErrorMessage = "DueDate is required.")]
    public DateOnly? DueDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate.HasValue && DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "Due date cannot be in the past.",
                new[] { nameof(DueDate) });
        }
    }
}
