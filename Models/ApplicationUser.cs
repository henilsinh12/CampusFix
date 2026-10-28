using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CampusFix.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string FullName { get; set; } = string.Empty;
}
