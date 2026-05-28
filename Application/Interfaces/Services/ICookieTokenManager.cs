using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services;

public interface ICookieTokenManager
{
    public string? GetRefreshToken();

    public void SetRefreshToken(string token, DateTimeOffset expiresAt);

    public void DeleteRefreshToken();
}