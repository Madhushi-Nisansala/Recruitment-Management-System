using Xunit;

namespace RecruitmentManagementSystem.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
