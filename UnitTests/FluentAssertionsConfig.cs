using FluentAssertions;
using FluentAssertions.Extensibility;
using UnitTests;

[assembly: AssertionEngineInitializer(
    typeof(AssertionEngineConfiguration),
    nameof(AssertionEngineConfiguration.AcknowledgeSoftWarning))]

namespace UnitTests
{
    public static class AssertionEngineConfiguration
    {
        public static void AcknowledgeSoftWarning()
        {
            License.Accepted = true;
        }
    }
}