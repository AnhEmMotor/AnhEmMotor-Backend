using Application.ApiContracts.Auth.Requests;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class IdentityService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<UserAuthDTO> AuthenticateAsync(string usernameOrEmail, string password)
        {
            ApplicationUser? user;

            // Logic check @ move về đây
            if (usernameOrEmail.Contains('@'))
            {
                user = await userManager.FindByEmailAsync(usernameOrEmail);
            }
            else
            {
                user = await userManager.FindByNameAsync(usernameOrEmail);
            }

            if (user is null)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            // Logic check status move về đây
            if (user.Status != UserStatus.Active || user.DeletedAt is not null)
            {
                throw new UnauthorizedException("Account is not available.");
            }

            // Logic check pass move về đây
            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            // Mapping từ Entity sang DTO
            var roles = await userManager.GetRolesAsync(user);
            return new UserAuthDTO() { Id = user.Id, Username = user.UserName, Roles = [.. roles], AuthMethods = ["amr"], Email = user.Email, FullName = user.FullName, Status = user.Status };
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null) return false;

            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

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

        // Infrastructure/Services/IdentityService.cs
        public async Task<UserAuthDTO> GetUserByRefreshTokenAsync(string refreshToken)
        {
            // Dùng UserManager để tìm user có token khớp (EF Core sẽ tự translate câu query này)
            // Lưu ý: UserManager không có hàm FindByRefreshToken, bạn phải query qua Users.
            // Nếu tuân thủ Clean Arc cực đoan, đoạn này nên gọi qua UserReadRepository, 
            // nhưng vì IdentityService nằm ở Infra nên chấp nhận dùng UserManager.

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken) ?? throw new UnauthorizedException("Invalid refresh token.");
            if (user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedException("Refresh token has expired. Please login again.");
            }

            if (user.Status != "Active" || user.DeletedAt != null)
            {
                throw new ForbiddenException("Account is not available.");
            }

            var roles = await userManager.GetRolesAsync(user);

            // Mapping sang DTO
            return new UserAuthDTO()
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = [.. roles],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status,
                AuthMethods = ["pwd"]
            };
        }
    }
}
