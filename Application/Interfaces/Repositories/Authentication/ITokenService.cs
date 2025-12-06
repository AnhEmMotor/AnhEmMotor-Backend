using Domain.Entities;

namespace Application.Interfaces.Repositories.Authentication;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user, string[] authMethods);
    string CreateRefreshToken();
    string? GetClaimFromToken(string token, string claimType);
}
