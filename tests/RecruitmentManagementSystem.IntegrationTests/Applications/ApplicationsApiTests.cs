using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Recruitment_Management_System.DTOs.Applications;
using RecruitmentManagementSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace RecruitmentManagementSystem.IntegrationTests.Applications;

[Collection("Integration")]
public sealed class ApplicationsApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Candidate_CanApplyToOpenJob_AndDuplicateIsRejected()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        var request = new { jobId = job.Id, coverLetter = "I am a strong fit." };
        var created = await client.PostAsJsonAsync("/api/applications", request);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var application = await created.Content.ReadFromJsonAsync<ApplicationResponseDto>();
        application!.CandidateId.Should().Be(candidate.Id);
        application.Status.Should().Be("Pending");
        application.AppliedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var duplicate = await client.PostAsJsonAsync("/api/applications", request);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Candidate_CannotApplyToMissingOrClosedJob()
    {
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        (await client.PostAsJsonAsync("/api/applications", new { jobId = 999999999 })).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var closedJob = await TestData.CreateJobAsync(factory, recruiter.Id, DateTime.UtcNow.AddDays(-1));
        (await client.PostAsJsonAsync("/api/applications", new { jobId = closedJob.Id })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Candidate_SeesOnlyOwnApplications()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var otherCandidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        await TestData.CreateApplicationAsync(factory, otherCandidate.Id, job.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        var applications = await client.GetFromJsonAsync<List<ApplicationResponseDto>>("/api/applications");
        applications.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task RecruiterCanUpdateStatusOnlyForOwnedJob()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        var application = await TestData.CreateApplicationAsync(factory, candidate.Id, job.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, recruiter);
        var response = await client.PutAsJsonAsync($"/api/applications/{application.Id}/status", new { status = "Accepted" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<ApplicationResponseDto>())!.Status.Should().Be("Accepted");
    }

    [Fact]
    public async Task CandidateCannotUpdateApplicationStatus()
    {
        var recruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, recruiter.Id);
        var application = await TestData.CreateApplicationAsync(factory, candidate.Id, job.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        (await client.PutAsJsonAsync($"/api/applications/{application.Id}/status", new { status = "Rejected" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
