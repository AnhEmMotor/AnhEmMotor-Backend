using FluentAssertions;

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(UnitTests.AssertionEngineInitializer),
    nameof(UnitTests.AssertionEngineInitializer.AcknowledgeSoftWarning))]

namespace UnitTests;

public static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning() { License.Accepted = true; }
}