using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories.User
{
    public class UserUpdateRepository(UserManager<ApplicationUser> userManager) : IUserUpdateRepository
    {
        public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiryTime)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
