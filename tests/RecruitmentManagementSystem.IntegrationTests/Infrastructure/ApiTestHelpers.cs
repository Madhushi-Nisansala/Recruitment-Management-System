using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Recruitment_Management_System.DTOs.Authentication;

namespace RecruitmentManagementSystem.IntegrationTests.Infrastructure;

public static class ApiTestHelpers
{
    public static async Task<AuthResponseDto> LoginAsync(HttpClient client, TestAccount account)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = account.Email,
            password = account.Password
        });
        response.IsSuccessStatusCode.Should().BeTrue(await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory,
        TestAccount account)
    {
        var client = factory.CreateClient();
        var auth = await LoginAsync(client, account);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(auth.TokenType, auth.AccessToken);
        return client;
    }
}
