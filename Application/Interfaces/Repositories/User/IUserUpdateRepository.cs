using System;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserUpdateRepository
    {
        public Task UpdateRefreshTokenAsync(
            Guid userId,
            string refreshToken,
            DateTimeOffset expiryTime,
            CancellationToken cancellationToken);
    }
}
