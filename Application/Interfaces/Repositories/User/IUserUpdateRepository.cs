using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserUpdateRepository
    {
        Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiryTime);
    }
}
