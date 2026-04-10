using Application.ApiContracts.Auth.Requests;
using Application.Common.Models;

namespace Application.Interfaces.Services;

public interface IExternalAuthService
{
    public Task<Result<ExternalUserDto>> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken);

    public Task<Result<ExternalUserDto>> ValidateFacebookTokenAsync(
        string accessToken,
        CancellationToken cancellationToken);
}
