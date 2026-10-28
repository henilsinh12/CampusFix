using CampusFix.Data;
using CampusFix.Models;
using CampusFix.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        var isStaff = User.IsInRole("Staff");
        var uid = users.GetUserId(User)!;
        var query = db.Complaints.Include(c => c.Category).Include(c => c.Student).Include(c => c.AssignedStaff).AsQueryable();
        if (!isAdmin && !isStaff) query = query.Where(c => c.StudentId == uid);
        if (isStaff && !isAdmin) query = query.Where(c => c.AssignedStaffId == uid);

        var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        var vm = new DashboardViewModel
        {
            Total = list.Count,
            Pending = list.Count(c => c.Status == "Pending"),
            InProgress = list.Count(c => c.Status == "In Progress"),
            Resolved = list.Count(c => c.Status == "Resolved"),
            HighPriority = list.Count(c => c.Priority == "High" || c.Priority == "Urgent"),
            RecentComplaints = list.Take(6).ToList()
        };
        return View(vm);
    }
}
