namespace Application.Interfaces.Repositories.Authentication;

public interface ICurrentUserService
{
    string? GetRefreshToken();

    void SetRefreshToken(string token);

    string? GetAuthorizationHeader();
}
