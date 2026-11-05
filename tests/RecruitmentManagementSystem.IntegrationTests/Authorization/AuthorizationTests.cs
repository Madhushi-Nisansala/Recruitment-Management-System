using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RecruitmentManagementSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace RecruitmentManagementSystem.IntegrationTests.Authorization;

[Collection("Integration")]
public sealed class AuthorizationTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task ProtectedJobCreation_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/jobs", new { title = "Unauthorized", description = "No token" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Candidate_CannotCreateJob()
    {
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        var response = await client.PostAsJsonAsync("/api/jobs", new { title = "Candidate Job", description = "Not permitted" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Recruiter_CannotUpdateAnotherRecruitersJob()
    {
        var owner = await TestData.CreateUserAsync(factory, "Recruiter");
        var otherRecruiter = await TestData.CreateUserAsync(factory, "Recruiter");
        var job = await TestData.CreateJobAsync(factory, owner.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, otherRecruiter);
        var response = await client.PutAsJsonAsync($"/api/jobs/{job.Id}", new
        {
            title = "Unauthorized update",
            description = "Should not be persisted",
            location = "Remote"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Candidate_CannotReadAnotherCandidatesApplication()
    {
        var owner = await TestData.CreateUserAsync(factory, "Recruiter");
        var candidate = await TestData.CreateUserAsync(factory, "Candidate");
        var otherCandidate = await TestData.CreateUserAsync(factory, "Candidate");
        var job = await TestData.CreateJobAsync(factory, owner.Id);
        var application = await TestData.CreateApplicationAsync(factory, otherCandidate.Id, job.Id);
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, candidate);
        var response = await client.GetAsync($"/api/applications/{application.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MalformedToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not-a-jwt");
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
