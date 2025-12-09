using Application.ApiContracts.Auth.Requests;

namespace Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<UserAuthDTO> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            CancellationToken cancellationToken);
    }
}
