using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;

namespace Application.Interfaces.Services
{
    public interface IIdentityService
    {
        public Task<Result<UserAuth>> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            CancellationToken cancellationToken);

        public Task<Result<UserAuth>> LoginWithExternalProviderAsync(
            ExternalUserDto externalUser,
            CancellationToken cancellationToken);
    }
}
