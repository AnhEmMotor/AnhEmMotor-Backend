using FluentAssertions;

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(IntegrationTests.AssertionEngineInitializer),
    nameof(IntegrationTests.AssertionEngineInitializer.AcknowledgeSoftWarning))]

namespace IntegrationTests;

public static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning() { License.Accepted = true; }
}