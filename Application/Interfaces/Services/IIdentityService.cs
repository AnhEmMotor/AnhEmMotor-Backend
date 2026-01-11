using Application.ApiContracts.Auth.Requests;
using Application.Common.Models;

namespace Application.Interfaces.Services
{
    public interface IIdentityService
    {
        public Task<Result<UserAuthDTO>> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            CancellationToken cancellationToken);
    }
}
