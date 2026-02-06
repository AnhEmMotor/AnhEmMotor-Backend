using Xunit;

namespace IntegrationTests.SetupClass;

// Class này không chứa code, chỉ dùng để định nghĩa Collection
[CollectionDefinition("Shared Integration Collection")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    // Để trống. Class này chỉ đóng vai trò như một cái nhãn (marker).
}
