using FluentAssertions;
using FluentAssertions.Extensibility;
using IntegrationTests;

[assembly: AssertionEngineInitializer(
    typeof(AssertionEngineConfiguration),
    nameof(AssertionEngineConfiguration.AcknowledgeSoftWarning))]

namespace IntegrationTests
{
    public static class AssertionEngineConfiguration
    {
        public static void AcknowledgeSoftWarning()
        {
            License.Accepted = true;
        }
    }
}