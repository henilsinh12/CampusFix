using CampusFix.Data;
using CampusFix.Models;
using CampusFix.Services;
using CampusFix.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers;

[Authorize]
public class ComplaintsController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users) : Controller
{
    private string UserId => users.GetUserId(User)!;
    private bool IsPrivileged => User.IsInRole("Admin") || User.IsInRole("Staff");

    private async Task LoadCategories(ComplaintFormViewModel vm)
    {
        vm.Categories = await db.ComplaintCategories.OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
    }

    public async Task<IActionResult> Index(string? search, string? status, string? priority)
    {
        var query = db.Complaints.Include(c => c.Category).Include(c => c.Student).Include(c => c.AssignedStaff).AsQueryable();
        if (User.IsInRole("Student")) query = query.Where(c => c.StudentId == UserId);
        else if (User.IsInRole("Staff")) query = query.Where(c => c.AssignedStaffId == UserId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || c.ReferenceNumber.Contains(search));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(c => c.Priority == priority);
        ViewBag.Search = search; ViewBag.Status = status; ViewBag.Priority = priority;
        return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ComplaintFormViewModel();
        await LoadCategories(vm);
        return View(vm);
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComplaintFormViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadCategories(vm); return View(vm); }
        var categoryExists = await db.ComplaintCategories.AnyAsync(c => c.Id == vm.CategoryId);
        if (!categoryExists) { ModelState.AddModelError(nameof(vm.CategoryId), "Choose a valid category."); await LoadCategories(vm); return View(vm); }

        var complaint = new Complaint
        {
            Title = vm.Title.Trim(), Description = vm.Description.Trim(), Location = vm.Location.Trim(),
            Priority = vm.Priority, CategoryId = vm.CategoryId, StudentId = UserId,
            ReferenceNumber = ComplaintReference.Create(), Status = "Pending", CreatedAt = DateTime.UtcNow
        };
        db.Complaints.Add(complaint);
        await db.SaveChangesAsync();
        db.ComplaintStatusHistories.Add(new ComplaintStatusHistory { ComplaintId = complaint.Id, OldStatus = "—", NewStatus = "Pending", Note = "Complaint submitted", ChangedById = UserId });
        await db.SaveChangesAsync();
        TempData["Success"] = $"Complaint submitted. Reference: {complaint.ReferenceNumber}";
        return RedirectToAction(nameof(Details), new { id = complaint.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var complaint = await db.Complaints.Include(c => c.Category).Include(c => c.Student)
            .Include(c => c.AssignedStaff).Include(c => c.StatusHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(c => c.Id == id);
        if (complaint is null) return NotFound();
        if (!IsPrivileged && complaint.StudentId != UserId) return Forbid();
        if (User.IsInRole("Staff") && !User.IsInRole("Admin") && complaint.AssignedStaffId != UserId) return Forbid();
        if (User.IsInRole("Admin"))
            ViewBag.StaffUsers = await users.GetUsersInRoleAsync("Staff");
        return View(complaint);
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var c = await db.Complaints.FirstOrDefaultAsync(c => c.Id == id);
        if (c is null) return NotFound();
        if (!User.IsInRole("Admin") && (c.StudentId != UserId || c.Status != "Pending")) return Forbid();
        var vm = new ComplaintFormViewModel { Id = c.Id, Title = c.Title, Description = c.Description, Location = c.Location, Priority = c.Priority, CategoryId = c.CategoryId };
        await LoadCategories(vm);
        return View(vm);
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ComplaintFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        var c = await db.Complaints.FirstOrDefaultAsync(c => c.Id == id);
        if (c is null) return NotFound();
        if (!User.IsInRole("Admin") && (c.StudentId != UserId || c.Status != "Pending")) return Forbid();
        if (!ModelState.IsValid) { await LoadCategories(vm); return View(vm); }
        c.Title = vm.Title.Trim(); c.Description = vm.Description.Trim(); c.Location = vm.Location.Trim();
        c.Priority = vm.Priority; c.CategoryId = vm.CategoryId; c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Complaint updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Student")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Complaints.FindAsync(id);
        if (c is null) return NotFound();
        if (!User.IsInRole("Admin") && (c.StudentId != UserId || c.Status != "Pending")) return Forbid();
        db.Complaints.Remove(c);
        await db.SaveChangesAsync();
        TempData["Success"] = "Complaint deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? resolutionNotes)
    {
        var allowed = new[] { "Pending", "In Progress", "Resolved", "Rejected" };
        if (!allowed.Contains(status)) return BadRequest("Invalid status.");
        var c = await db.Complaints.FindAsync(id);
        if (c is null) return NotFound();
        if (User.IsInRole("Staff") && !User.IsInRole("Admin") && c.AssignedStaffId != UserId) return Forbid();
        var oldStatus = c.Status;
        c.Status = status;
        c.UpdatedAt = DateTime.UtcNow;
        c.ResolvedAt = status == "Resolved" ? DateTime.UtcNow : null;
        c.ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        db.ComplaintStatusHistories.Add(new ComplaintStatusHistory
        {
            ComplaintId = c.Id, OldStatus = oldStatus, NewStatus = status,
            Note = c.ResolutionNotes, ChangedById = UserId, ChangedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        TempData["Success"] = $"Status changed to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStaff(int id, string? staffId)
    {
        var c = await db.Complaints.FindAsync(id);
        if (c is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(staffId))
        {
            var staff = await users.FindByIdAsync(staffId);
            if (staff is null || !await users.IsInRoleAsync(staff, "Staff")) return BadRequest("Select a valid staff member.");
        }
        c.AssignedStaffId = string.IsNullOrWhiteSpace(staffId) ? null : staffId;
        c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Staff assignment updated.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
