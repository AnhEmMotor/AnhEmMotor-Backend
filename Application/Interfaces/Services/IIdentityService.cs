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
    }
}
