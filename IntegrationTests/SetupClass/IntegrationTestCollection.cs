namespace IntegrationTests.SetupClass;

[CollectionDefinition("Shared Integration Collection")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
