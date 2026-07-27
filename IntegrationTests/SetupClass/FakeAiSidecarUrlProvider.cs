using Application.Interfaces.Services;

namespace IntegrationTests.SetupClass;

public class FakeAiSidecarUrlProvider : IAiSidecarUrlProvider
{
    public string GetSidecarUrl() => "http://127.0.0.1:1";
}
