using Microsoft.EntityFrameworkCore;
using Recruitment_Management_System.Models;

namespace Recruitment_Management_System.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<Application> Applications => Set<Application>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table => table.HasCheckConstraint(
                "CK_Users_Role",
                "[Role] IN ('Candidate', 'Recruiter', 'Admin')"));

            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FullName).IsRequired().HasMaxLength(100);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(150);
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.Role).IsRequired().HasMaxLength(30);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("Jobs", table =>
            {
                table.HasCheckConstraint(
                    "CK_Jobs_SalaryRange",
                    "[SalaryMin] IS NULL OR [SalaryMax] IS NULL OR [SalaryMin] <= [SalaryMax]");
                table.HasCheckConstraint(
                    "CK_Jobs_ClosingDate",
                    "[ClosingDate] IS NULL OR [ClosingDate] > [PostedAt]");
            });

            entity.HasKey(job => job.Id);
            entity.Property(job => job.Title).IsRequired().HasMaxLength(150);
            entity.Property(job => job.Description).IsRequired();
            entity.Property(job => job.Location).HasMaxLength(100);
            entity.Property(job => job.EmploymentType).HasMaxLength(50);
            entity.Property(job => job.SalaryMin).HasPrecision(18, 2);
            entity.Property(job => job.SalaryMax).HasPrecision(18, 2);
            entity.Property(job => job.PostedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(job => job.Recruiter)
                .WithMany(user => user.Jobs)
                .HasForeignKey(job => job.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Applications");

            entity.HasKey(application => application.Id);
            entity.HasIndex(application => new { application.CandidateId, application.JobId })
                .IsUnique();
            entity.Property(application => application.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(application => application.CoverLetter).HasMaxLength(2000);
            entity.Property(application => application.CvFilePath).HasMaxLength(500);
            entity.Property(application => application.AppliedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(application => application.Candidate)
                .WithMany(user => user.Applications)
                .HasForeignKey(application => application.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(application => application.Job)
                .WithMany(job => job.Applications)
                .HasForeignKey(application => application.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
