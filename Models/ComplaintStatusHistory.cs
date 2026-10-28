using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models;

public class ComplaintStatusHistory
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public Complaint? Complaint { get; set; }
    [Required, StringLength(30)]
    public string OldStatus { get; set; } = string.Empty;
    [Required, StringLength(30)]
    public string NewStatus { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Note { get; set; }
    public string ChangedById { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
