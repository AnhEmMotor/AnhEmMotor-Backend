using Domain.Entities;

namespace Application.Interfaces.Repositories.Authentication;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user, string[] authMethods, CancellationToken cancellationToken);

    string CreateRefreshToken();

    string? GetClaimFromToken(string token, string claimType);
}
