using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(
    UserManager<ApplicationUser> userManager,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<ChangeUserStatusCommand, ChangeStatusUserByManagerResponse>
{
    public async Task<ChangeStatusUserByManagerResponse> Handle(
        ChangeUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        if(!UserStatus.IsValid(request.Model.Status))
        {
            throw new ValidationException([ new ValidationFailure("Status", "Status not vaild, please check.") ]);
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        cancellationToken.ThrowIfCancellationRequested();

        if(string.Compare(request.Model.Status, UserStatus.Banned) == 0)
        {
            var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

            if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            {
                throw new ValidationException([ new ValidationFailure("Email", "Cannot deactivate protected user.") ]);
            }

            var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
            var userRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

            foreach(var userRole in userRoles)
            {
                if(superRoles.Contains(userRole))
                {
                    var usersInRole = await userManager.GetUsersInRoleAsync(userRole).ConfigureAwait(false);
                    var activeUsersInRole = usersInRole.Where(u => string.Compare(u.Status, UserStatus.Active) == 0)
                        .ToList();

                    if(activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == request.UserId)
                    {
                        throw new ValidationException(
                            [ new ValidationFailure(
                                "Status",
                                $"Cannot deactivate user. This is the last active user with SuperRole '{userRole}'.") ]);
                    }
                }
            }
        }

        user.Status = request.Model.Status;
        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);
        if(!result.Succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in result.Errors)
            {
                string fieldName = Common.Helper.IdentityHelper.GetFieldForIdentityError(error.Code);
                failures.Add(new ValidationFailure(fieldName, error.Description));
            }
            throw new ValidationException(failures);
        }

        return new ChangeStatusUserByManagerResponse()
        {
            Message = $"User status changed to {request.Model.Status} successfully.",
        };
    }
}
