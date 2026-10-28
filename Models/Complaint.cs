using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models;

public class Complaint
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 15)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Normal";

    public string Status { get; set; } = "Pending";
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
    public ComplaintCategory? Category { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public string? AssignedStaffId { get; set; }
    public ApplicationUser? AssignedStaff { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    public ICollection<ComplaintStatusHistory> StatusHistory { get; set; } = new List<ComplaintStatusHistory>();
}
