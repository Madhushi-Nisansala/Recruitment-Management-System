using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Recruitment_Management_System.Data;

namespace RecruitmentManagementSystem.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestSigningKey = "test-only-signing-key-at-least-32-characters-long";
    public string DatabaseName { get; } = $"RecruitmentManagementDB_IntegrationTests_{Guid.NewGuid():N}";
    public string ConnectionString =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Issuer"] = "RecruitmentManagementSystem.Tests",
                ["Jwt:Audience"] = "RecruitmentManagementSystem.Tests.Client",
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Authentication:AllowRecruiterRegistration"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            var optionsDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (optionsDescriptor is not null)
            {
                services.Remove(optionsDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            using var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureDeleted();
        }

        base.Dispose(disposing);
    }
}
