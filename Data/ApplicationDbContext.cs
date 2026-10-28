using CampusFix.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintCategory> ComplaintCategories => Set<ComplaintCategory>();
    public DbSet<ComplaintStatusHistory> ComplaintStatusHistories => Set<ComplaintStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Complaint>().HasIndex(c => c.ReferenceNumber).IsUnique();
        builder.Entity<Complaint>().Property(c => c.Title).HasMaxLength(100).IsRequired();
        builder.Entity<Complaint>().Property(c => c.Description).HasMaxLength(2000).IsRequired();
        builder.Entity<Complaint>().Property(c => c.Location).HasMaxLength(150).IsRequired();
        builder.Entity<Complaint>()
            .HasOne(c => c.Category).WithMany(c => c.Complaints)
            .HasForeignKey(c => c.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Complaint>()
            .HasOne(c => c.Student).WithMany()
            .HasForeignKey(c => c.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Complaint>()
            .HasOne(c => c.AssignedStaff).WithMany()
            .HasForeignKey(c => c.AssignedStaffId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<ComplaintStatusHistory>()
            .HasOne(h => h.Complaint).WithMany(c => c.StatusHistory)
            .HasForeignKey(h => h.ComplaintId).OnDelete(DeleteBehavior.Cascade);
    }
}
