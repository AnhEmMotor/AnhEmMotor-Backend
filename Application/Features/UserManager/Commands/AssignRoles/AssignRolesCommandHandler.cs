using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.UserManager.Commands.AssignRoles;

public class AssignRolesCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration) : IRequestHandler<AssignRolesCommand, AssignRoleResponse>
{
    public async Task<AssignRoleResponse> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        foreach(string role in request.Model.RoleNames)
        {
            if(string.IsNullOrWhiteSpace(role))
            {
                throw new ValidationException(
                    [ new ValidationFailure("RoleNames", "Role names cannot contain empty or whitespace values.") ]);
            }
            if(UserStatus.IsValid(role))
            {
                throw new ValidationException([ new ValidationFailure("RoleNames", "Role names not vaild.") ]);
            }
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        var invalidRoles = new List<string>();
        foreach(var roleName in request.Model.RoleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
            if(!roleExists)
            {
                invalidRoles.Add(roleName);
            }
        }

        if(invalidRoles.Count > 0)
        {
            throw new ValidationException(
                [ new ValidationFailure(
                    "RoleNames",
                    $"The following roles do not exist: {string.Join(", ", invalidRoles)}") ]);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var currentRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];

        var rolesToRemove = currentRoles.Except(request.Model.RoleNames).ToList();
        foreach(var roleToRemove in rolesToRemove)
        {
            if(superRoles.Contains(roleToRemove))
            {
                var usersWithThisRole = await userManager.GetUsersInRoleAsync(roleToRemove).ConfigureAwait(false);
                if(usersWithThisRole.Count == 1 && usersWithThisRole[0].Id == request.UserId)
                {
                    throw new ValidationException(
                        [ new ValidationFailure(
                            "RoleNames",
                            $"Cannot remove SuperRole '{roleToRemove}' from user. This is the last user with this role.") ]);
                }
            }
        }

        if(currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles).ConfigureAwait(false);
            if(!removeResult.Succeeded)
            {
                var failures = new List<ValidationFailure>();
                foreach(var error in removeResult.Errors)
                {
                    string fieldName = Common.Helper.IdentityHelper.GetFieldForIdentityError(error.Code);
                    failures.Add(new ValidationFailure(fieldName, error.Description));
                }
                throw new ValidationException(failures);
            }
        }

        if(request.Model.RoleNames.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, request.Model.RoleNames).ConfigureAwait(false);
            if(!addResult.Succeeded)
            {
                var failures = new List<ValidationFailure>();
                foreach(var error in addResult.Errors)
                {
                    string fieldName = Common.Helper.IdentityHelper.GetFieldForIdentityError(error.Code);
                    failures.Add(new ValidationFailure(fieldName, error.Description));
                }
                throw new ValidationException(failures);
            }
        }

        var updatedRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new AssignRoleResponse()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Roles = updatedRoles
        };
    }
}
