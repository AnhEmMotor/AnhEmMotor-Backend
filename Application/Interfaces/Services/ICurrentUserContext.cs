using System;

namespace Application.Interfaces.Services;

public interface ICurrentUserContext
{
    public string GetAuthorizationHeader();

    public Guid GetUserId();

    public string GetAccessToken();
}
