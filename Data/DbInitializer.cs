using CampusFix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles if they do not exist.
        foreach (var role in new[] { "Student", "Staff", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult =
                    await roleManager.CreateAsync(new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create role {role}: " +
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // Ensure demo accounts exist with known passwords and correct roles.
        await EnsureDemoUser(
            userManager,
            "admin@campusfix.local",
            "ChangeMe123!",
            "CampusFix Administrator",
            "Admin");

        await EnsureDemoUser(
            userManager,
            "staff@campusfix.local",
            "Staff123!",
            "CampusFix Maintenance Staff",
            "Staff");

        await EnsureDemoUser(
            userManager,
            "student@campusfix.local",
            "Student123!",
            "Demo Student",
            "Student");

        // Create default complaint categories only when none exist.
        if (!await db.ComplaintCategories.AnyAsync())
        {
            db.ComplaintCategories.AddRange(
                new ComplaintCategory
                {
                    Name = "Electrical",
                    Description = "Fans, lights, switches and wiring"
                },
                new ComplaintCategory
                {
                    Name = "IT & Wi-Fi",
                    Description = "Network, internet and computer issues"
                },
                new ComplaintCategory
                {
                    Name = "Furniture",
                    Description = "Benches, desks, chairs and fixtures"
                },
                new ComplaintCategory
                {
                    Name = "Cleanliness",
                    Description = "Classrooms and campus sanitation"
                },
                new ComplaintCategory
                {
                    Name = "Water & Plumbing",
                    Description = "Water supply and plumbing"
                },
                new ComplaintCategory
                {
                    Name = "Laboratory",
                    Description = "Lab equipment and facilities"
                }
            );

            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureDemoUser(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create demo account {email}: " +
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Update the existing demo account without deleting it.
            user.EmailConfirmed = true;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not update demo account {email}: " +
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            // Reset the demo password if the account already exists.
            var token =
                await userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult =
                await userManager.ResetPasswordAsync(user, token, password);

            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not reset password for {email}: " +
                    string.Join(", ",
                        resetResult.Errors.Select(e => e.Description)));
            }
        }

        // Ensure the account has the required role.
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult =
                await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign role {role} to {email}: " +
                    string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}