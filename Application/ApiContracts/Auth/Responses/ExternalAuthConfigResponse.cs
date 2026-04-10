
namespace Application.ApiContracts.Auth.Responses;

public class ExternalAuthConfigResponse
{
    public string GoogleClientId { get; set; } = string.Empty;

    public string FacebookAppId { get; set; } = string.Empty;
}
