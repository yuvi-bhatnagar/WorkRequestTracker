using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkRequestTracker.Api.Models;

public class WorkRequestNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int WorkRequestId { get; set; }

    [ForeignKey(nameof(WorkRequestId))]
    public WorkRequest WorkRequest { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
