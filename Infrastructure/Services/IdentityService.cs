using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class IdentityService(UserManager<ApplicationUser> userManager, IConfiguration configuration) : IIdentityService
{
    public async Task<Result<UserAuth>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? user;

        if(usernameOrEmail.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(usernameOrEmail).ConfigureAwait(false);
        } else
        {
            user = await userManager.FindByNameAsync(usernameOrEmail).ConfigureAwait(false);
        }

        if(user == null)
        {
            return Error.Unauthorized("Wrong username/email or password.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);

        if(!isPasswordValid)
        {
            return Error.Unauthorized("Wrong username/email or password.");
        }

        if(string.Compare(user.Status, UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User is banned.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserAuth
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = [ .. roles ],
            AuthMethods = [ "amr" ],
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            SecurityStamp = user.SecurityStamp
        };
    }

    public async Task<Result<UserAuth>> LoginWithExternalProviderAsync(
        ExternalUserDto externalUser,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(externalUser);

        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(externalUser.Email).ConfigureAwait(false);

        if(user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = externalUser.Email,
                Email = externalUser.Email,
                FullName = externalUser.Name,
                AvatarUrl = externalUser.Picture,
                Status = UserStatus.Active,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(user).ConfigureAwait(false);
            if(!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => Error.Failure(e.Description)).ToList();
                return Result<UserAuth>.Failure(errors);
            }

            var defaultRoles = configuration.GetSection("ProtectedAuthorizationEntities:DefaultRolesForNewUsers")
                .Get<string[]>();
            if(defaultRoles is not null && defaultRoles.Length > 0)
            {
                await userManager.AddToRolesAsync(user, defaultRoles).ConfigureAwait(false);
            }
        } else
        {
            if(string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(externalUser.Picture))
            {
                user.AvatarUrl = externalUser.Picture;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }
        }

        if(string.Compare(user.Status, UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User is banned.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserAuth
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = [ .. roles ],
            AuthMethods = [ externalUser.Provider.ToLower() ],
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            SecurityStamp = user.SecurityStamp
        };
    }
}