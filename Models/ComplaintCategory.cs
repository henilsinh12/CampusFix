using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models;

public class ComplaintCategory
{
    public int Id { get; set; }
    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;
    [StringLength(250)]
    public string? Description { get; set; }
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
