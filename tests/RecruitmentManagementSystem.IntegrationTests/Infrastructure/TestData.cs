using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.Models;

namespace RecruitmentManagementSystem.IntegrationTests.Infrastructure;

public sealed record TestAccount(int Id, string FullName, string Email, string Password, string Role);

public static class TestData
{
    public static async Task<TestAccount> CreateUserAsync(
        CustomWebApplicationFactory factory,
        string role,
        string? email = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var account = new TestAccount(
            0,
            $"Integration {role}",
            email ?? $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test",
            "IntegrationPassword123!",
            role);
        var user = new User
        {
            FullName = account.FullName,
            Email = account.Email.ToUpperInvariant(),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, account.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return account with { Id = user.Id };
    }

    public static async Task<Job> CreateJobAsync(
        CustomWebApplicationFactory factory,
        int recruiterId,
        DateTime? closingDate = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var job = new Job
        {
            Title = $"Integration Job {Guid.NewGuid():N}",
            Description = "A job created by an integration test.",
            Location = "Remote",
            EmploymentType = "Full-time",
            SalaryMin = 50000,
            SalaryMax = 80000,
            PostedAt = DateTime.UtcNow,
            ClosingDate = closingDate,
            RecruiterId = recruiterId
        };
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync();
        return job;
    }

    public static async Task<Application> CreateApplicationAsync(
        CustomWebApplicationFactory factory,
        int candidateId,
        int jobId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var application = new Application
        {
            CandidateId = candidateId,
            JobId = jobId,
            AppliedAt = DateTime.UtcNow,
            Status = "Pending",
            CoverLetter = "Integration test cover letter."
        };
        dbContext.Applications.Add(application);
        await dbContext.SaveChangesAsync();
        return application;
    }
}
