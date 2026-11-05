using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Recruitment_Management_System.Models;

namespace Recruitment_Management_System.Data;

public static class DevelopmentDataSeeder
{
    private const string DemoRecruiterEmail = "DEMO.RECRUITER@RECRUITFLOW.LOCAL";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("SeedData:Enabled"))
        {
            return;
        }

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var recruiterEmail = configuration["SeedData:RecruiterEmail"]?.Trim().ToUpperInvariant();
        var recruiterPassword = configuration["SeedData:RecruiterPassword"];
        var resetRecruiterPassword = configuration.GetValue<bool>("SeedData:ResetRecruiterPassword");

        if (string.IsNullOrWhiteSpace(recruiterEmail) || string.IsNullOrWhiteSpace(recruiterPassword))
        {
            throw new InvalidOperationException(
                "Development seed data is enabled, but SeedData:RecruiterEmail or SeedData:RecruiterPassword is missing. Configure the password with user secrets.");
        }

        if (resetRecruiterPassword && !string.Equals(recruiterEmail, DemoRecruiterEmail, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"SeedData:ResetRecruiterPassword can only target the configured development demo account '{DemoRecruiterEmail}'.");
        }

        var recruiter = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Email == recruiterEmail, cancellationToken);

        if (resetRecruiterPassword)
        {
            if (recruiter is null)
            {
                throw new InvalidOperationException(
                    $"The development demo Recruiter account '{DemoRecruiterEmail}' does not exist. The password reset will not create an account.");
            }

            if (!string.Equals(recruiter.Role, "Recruiter", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The development demo account '{DemoRecruiterEmail}' is not a Recruiter. The password reset was not applied.");
            }

            var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
            recruiter.PasswordHash = passwordHasher.HashPassword(recruiter, recruiterPassword);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (recruiter is null && !resetRecruiterPassword)
        {
            recruiter = new User
            {
                FullName = "RecruitFlow Demo Recruiter",
                Email = recruiterEmail,
                Role = "Recruiter",
                CreatedAt = DateTime.UtcNow
            };

            var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
            recruiter.PasswordHash = passwordHasher.HashPassword(recruiter, recruiterPassword);
            dbContext.Users.Add(recruiter);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (recruiter is not null && !string.Equals(recruiter.Role, "Recruiter", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The configured seed email '{recruiterEmail}' belongs to a non-Recruiter account. Choose a different SeedData:RecruiterEmail.");
        }

        var sampleJobs = CreateSampleJobs(recruiter.Id);
        var existingTitles = await dbContext.Jobs
            .Where(job => sampleJobs.Select(sample => sample.Title).Contains(job.Title))
            .Select(job => job.Title)
            .ToListAsync(cancellationToken);

        var missingJobs = sampleJobs
            .Where(job => !existingTitles.Contains(job.Title, StringComparer.Ordinal))
            .ToList();

        if (missingJobs.Count > 0)
        {
            dbContext.Jobs.AddRange(missingJobs);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static IReadOnlyList<Job> CreateSampleJobs(int recruiterId)
    {
        var now = DateTime.UtcNow;
        return
        [
            new Job
            {
                Title = "Senior .NET Backend Developer",
                Description = "Design and build reliable ASP.NET Core APIs, work with Entity Framework Core, and collaborate with product and frontend teams to deliver scalable recruitment technology.",
                Location = "Colombo, Sri Lanka",
                EmploymentType = "Full-time",
                SalaryMin = 1800000,
                SalaryMax = 2800000,
                PostedAt = now.AddDays(-2),
                ClosingDate = now.AddDays(35),
                RecruiterId = recruiterId
            },
            new Job
            {
                Title = "React Frontend Engineer",
                Description = "Create accessible and responsive React experiences using TypeScript, modern component patterns, and API integrations that help candidates and recruiters work efficiently.",
                Location = "Hybrid - Colombo",
                EmploymentType = "Full-time",
                SalaryMin = 1500000,
                SalaryMax = 2400000,
                PostedAt = now.AddDays(-4),
                ClosingDate = now.AddDays(28),
                RecruiterId = recruiterId
            },
            new Job
            {
                Title = "Product Design Intern",
                Description = "Support user research, wireframing, and usability testing for a growing recruitment platform while learning from experienced product and engineering teams.",
                Location = "Remote",
                EmploymentType = "Internship",
                SalaryMin = 300000,
                SalaryMax = 500000,
                PostedAt = now.AddDays(-6),
                ClosingDate = now.AddDays(21),
                RecruiterId = recruiterId
            },
            new Job
            {
                Title = "QA Automation Engineer",
                Description = "Develop maintainable automated tests for web APIs and React workflows, investigate defects, and improve release confidence across the product lifecycle.",
                Location = "Kandy, Sri Lanka",
                EmploymentType = "Full-time",
                SalaryMin = 1200000,
                SalaryMax = 2000000,
                PostedAt = now.AddDays(-8),
                ClosingDate = now.AddDays(42),
                RecruiterId = recruiterId
            },
            new Job
            {
                Title = "Technical Recruiter",
                Description = "Partner with hiring managers, source technical talent, guide candidates through the process, and help build a positive and inclusive recruitment experience.",
                Location = "Hybrid - Galle",
                EmploymentType = "Full-time",
                SalaryMin = 1000000,
                SalaryMax = 1700000,
                PostedAt = now.AddDays(-10),
                ClosingDate = now.AddDays(30),
                RecruiterId = recruiterId
            }
        ];
    }
}
