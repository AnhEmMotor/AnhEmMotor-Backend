using ControllerTests;
using FluentAssertions;
using FluentAssertions.Extensibility;

[assembly: AssertionEngineInitializer(
    typeof(AssertionEngineConfiguration),
    nameof(AssertionEngineConfiguration.AcknowledgeSoftWarning))]

namespace ControllerTests
{
    public static class AssertionEngineConfiguration
    {
        public static void AcknowledgeSoftWarning()
        {
            License.Accepted = true;
        }
    }
}