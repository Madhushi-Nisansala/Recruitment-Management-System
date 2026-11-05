using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.DTOs.Authentication;
using Recruitment_Management_System.Models;
using RecruitmentManagementSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace RecruitmentManagementSystem.IntegrationTests.Authentication;

[Collection("Integration")]
public sealed class AuthenticationTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Register_ValidCandidate_CreatesHashedUser()
    {
        using var client = factory.CreateClient();
        var email = $"candidate-{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test Candidate",
            email,
            password = "CandidatePassword123!",
            confirmPassword = "CandidatePassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        user!.Role.Should().Be("Candidate");
        user.Email.Should().Be(email.ToUpperInvariant());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = dbContext.Users.Single(x => x.Email == email.ToUpperInvariant());
        stored.PasswordHash.Should().NotBe("CandidatePassword123!");
        stored.PasswordHash.Should().NotBeNullOrWhiteSpace();
        scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>()
            .VerifyHashedPassword(stored, stored.PasswordHash, "CandidatePassword123!")
            .Should().NotBe(PasswordVerificationResult.Failed);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var client = factory.CreateClient();
        var email = $"duplicate-{Guid.NewGuid():N}@example.test";
        var request = new { fullName = "Duplicate User", email, password = "CandidatePassword123!", confirmPassword = "CandidatePassword123!" };
        (await client.PostAsJsonAsync("/api/auth/register", request)).StatusCode.Should().Be(HttpStatusCode.Created);
        var duplicate = await client.PostAsJsonAsync("/api/auth/register", request);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_AdminRole_IsRejected()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Unauthorized Admin",
            email = $"admin-{Guid.NewGuid():N}@example.test",
            password = "CandidatePassword123!",
            confirmPassword = "CandidatePassword123!",
            requestedRole = "Admin"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_InvalidRequest_ReturnsBadRequest()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "",
            email = "not-an-email",
            password = "short",
            confirmPassword = "different"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndMeReturnsSafeUser()
    {
        var account = await TestData.CreateUserAsync(factory, "Candidate");
        using var client = await ApiTestHelpers.CreateAuthenticatedClientAsync(factory, account);
        var me = await client.GetAsync("/api/auth/me");

        me.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await me.Content.ReadFromJsonAsync<UserResponseDto>();
        user!.Id.Should().Be(account.Id);
        user.Role.Should().Be("Candidate");
        (await me.Content.ReadAsStringAsync()).Should().NotContain("PasswordHash");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var account = await TestData.CreateUserAsync(factory, "Candidate");
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = account.Email, password = "WrongPassword123!" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
