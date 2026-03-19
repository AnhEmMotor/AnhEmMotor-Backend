using FluentAssertions;
using FluentAssertions.Extensibility;
using UnitTests;

[assembly: AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

namespace UnitTests;

public static class AssertionEngineInitializer
{
    public static void AcknowledgeSoftWarning() { License.Accepted = true; }
}