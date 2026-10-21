using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkRequestTracker.Api.Models;

public class WorkRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Priority Priority { get; set; }

    [Required]
    public WorkRequestStatus Status { get; set; }

    [Required]
    public DateOnly DueDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<WorkRequestNote> Notes { get; set; } = new List<WorkRequestNote>();
}
