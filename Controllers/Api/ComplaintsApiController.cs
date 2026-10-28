using CampusFix.Data;
using CampusFix.Models;
using CampusFix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers.Api;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsApiController(ApplicationDbContext db, UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = db.Complaints.Include(c => c.Category).AsQueryable();
        if (User.IsInRole("Student")) query = query.Where(c => c.StudentId == users.GetUserId(User));
        else if (User.IsInRole("Staff")) query = query.Where(c => c.AssignedStaffId == users.GetUserId(User));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        var data = await query.OrderByDescending(c => c.CreatedAt)
            .Select(c => new { c.Id, c.ReferenceNumber, c.Title, Category = c.Category!.Name, c.Priority, c.Status, c.Location, c.CreatedAt })
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await db.Complaints.Include(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
        if (c is null) return NotFound();
        if (User.IsInRole("Student") && c.StudentId != users.GetUserId(User)) return Forbid();
        if (User.IsInRole("Staff") && !User.IsInRole("Admin") && c.AssignedStaffId != users.GetUserId(User)) return Forbid();
        return Ok(new { c.Id, c.ReferenceNumber, c.Title, c.Description, Category = c.Category!.Name, c.Priority, c.Status, c.Location, c.CreatedAt, c.UpdatedAt });
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!await db.ComplaintCategories.AnyAsync(c => c.Id == request.CategoryId)) return BadRequest("Invalid category.");
        var c = new Complaint
        {
            Title = request.Title.Trim(), Description = request.Description.Trim(), Location = request.Location.Trim(),
            Priority = request.Priority, CategoryId = request.CategoryId, StudentId = users.GetUserId(User)!,
            ReferenceNumber = ComplaintReference.Create(), Status = "Pending"
        };
        db.Complaints.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { c.Id, c.ReferenceNumber, c.Status });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        if (request.Status is not ("Pending" or "In Progress" or "Resolved" or "Rejected"))
            return BadRequest("Invalid status.");
        var c = await db.Complaints.FindAsync(id);
        if (c is null) return NotFound();
        if (User.IsInRole("Staff") && !User.IsInRole("Admin") && c.AssignedStaffId != users.GetUserId(User)) return Forbid();
        c.Status = request.Status;
        c.UpdatedAt = DateTime.UtcNow;
        c.ResolvedAt = request.Status == "Resolved" ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return Ok(new { c.Id, c.ReferenceNumber, c.Status, c.UpdatedAt });
    }

    public record CreateComplaintRequest(
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 5)] string Title,
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(2000, MinimumLength = 15)] string Description,
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(150)] string Location,
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)] int CategoryId,
        [System.ComponentModel.DataAnnotations.RegularExpression("^(Low|Normal|High|Urgent)$")] string Priority = "Normal");
    public record UpdateStatusRequest([System.ComponentModel.DataAnnotations.Required] string Status);
}
