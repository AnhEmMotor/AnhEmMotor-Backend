using System;

namespace Application.Interfaces.Services;

public interface ICookieTokenManager
{
    public string? GetRefreshToken();

    public void SetRefreshToken(string token, DateTimeOffset expiresAt);

    public void DeleteRefreshToken();
}