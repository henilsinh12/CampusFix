using CampusFix.Data;
using CampusFix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Categories()
        => View(await db.ComplaintCategories.Include(c => c.Complaints).OrderBy(c => c.Name).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string? description)
    {
        name = (name ?? "").Trim();
        if (name.Length < 2 || name.Length > 60)
        {
            TempData["Error"] = "Category name must be between 2 and 60 characters.";
            return RedirectToAction(nameof(Categories));
        }
        if (await db.ComplaintCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
        {
            TempData["Error"] = "That category already exists.";
            return RedirectToAction(nameof(Categories));
        }
        db.ComplaintCategories.Add(new ComplaintCategory { Name = name, Description = description?.Trim() });
        await db.SaveChangesAsync();
        TempData["Success"] = "Category added.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.ComplaintCategories.Include(c => c.Complaints).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();
        if (category.Complaints.Count > 0)
        {
            TempData["Error"] = "This category is in use and cannot be deleted.";
            return RedirectToAction(nameof(Categories));
        }
        db.ComplaintCategories.Remove(category);
        await db.SaveChangesAsync();
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Users()
    {
        var all = await users.Users.OrderBy(u => u.FullName).ToListAsync();
        var result = new List<(ApplicationUser User, string Role)>();
        foreach (var user in all)
        {
            var roles = await users.GetRolesAsync(user);
            result.Add((user, roles.FirstOrDefault() ?? "No role"));
        }
        return View(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        if (role is not ("Student" or "Staff" or "Admin")) return BadRequest();
        var user = await users.FindByIdAsync(userId);
        if (user is null) return NotFound();
        if (user.Id == users.GetUserId(User) && role != "Admin")
        {
            TempData["Error"] = "You cannot remove your own administrator role.";
            return RedirectToAction(nameof(Users));
        }
        var oldRoles = await users.GetRolesAsync(user);
        await users.RemoveFromRolesAsync(user, oldRoles);
        await users.AddToRoleAsync(user, role);
        TempData["Success"] = $"Role updated for {user.Email}.";
        return RedirectToAction(nameof(Users));
    }
}
