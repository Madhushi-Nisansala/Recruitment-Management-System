using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.DTOs.Jobs;
using RecruitmentManagementSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace RecruitmentManagementSystem.IntegrationTests.Jobs;

[Collection("Integration")]
public sealed class JobsApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task GetJobs_ReturnsPaginatedJobs()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        await TestData.CreateJobAsync(factory, recruiter.Id);
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/jobs?pageNumber=1&pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobPageResponse>();
        result!.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetJob_MissingId_ReturnsNotFound()
    {
        using var client = factory.CreateClient();
        (await client.GetAsync("/api/jobs/999999999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Recruiter_CanCreateAndUpdateOwnedJob()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, recruiter);
        var create = await client.PostAsJsonAsync("/api/jobs", new
        {
            title = "Integration API Engineer",
            description = "Build APIs.",
            location = "Remote",
            employmentType = "Full-time",
            salaryMin = 70000,
            salaryMax = 90000,
            closingDate = DateTime.UtcNow.AddDays(30)
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<JobResponseDto>();
        created!.Recruiter.Id.Should().Be(recruiter.Id);

        var update = await client.PutAsJsonAsync($"/api/jobs/{created.Id}", new
        {
            title = "Senior Integration API Engineer",
            description = "Build reliable APIs.",
            salaryMin = 80000,
            salaryMax = 100000,
            closingDate = DateTime.UtcNow.AddDays(45)
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<JobResponseDto>())!.Title.Should().Be("Senior Integration API Engineer");

        using var scope = factory.Services.CreateScope();
        var persisted = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Jobs.Find(created.Id);
        persisted!.Title.Should().Be("Senior Integration API Engineer");
    }

    [Fact]
    public async Task InvalidSalaryRange_ReturnsBadRequest()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, recruiter);
        var response = await client.PostAsJsonAsync("/api/jobs", new
        {
            title = "Invalid salary",
            description = "Invalid range.",
            salaryMin = 100000,
            salaryMax = 50000
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JobWithApplications_CannotBeDeleted()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        await TestData.CreateApplicationAsync(factory, candidate.Id, job.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, recruiter);
        (await client.DeleteAsync($"/api/jobs/{job.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task JobWithoutApplications_CanBeDeletedByOwner()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, recruiter);
        (await client.DeleteAsync($"/api/jobs/{job.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record JobPageResponse(int PageNumber, int PageSize, int TotalCount, int TotalPages, List<JobResponseDto> Items);
}
