using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IRequestHandler<ChangeUserStatusCommand, ChangeStatusByManagerResponse>
{
    public async Task<ChangeStatusByManagerResponse> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (!UserStatus.IsValid(request.Model.Status))
        {
            throw new ValidationException([new ValidationFailure("Status", "Status not vaild, please check.")]);
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (request.Model.Status == UserStatus.Inactive)
        {
            var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                    .Get<List<string>>() ?? [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

            if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            {
                throw new ValidationException([new ValidationFailure("Email", "Cannot deactivate protected user.")]);
            }

            var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles")
                    .Get<List<string>>() ?? [];
            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                if (superRoles.Contains(userRole))
                {
                    var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                    var activeUsersInRole = usersInRole.Where(u => u.Status == UserStatus.Active).ToList();

                    if (activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == request.UserId)
                    {
                        throw new ValidationException([new ValidationFailure("Status", $"Cannot deactivate user. This is the last active user with SuperRole '{userRole}'.")]);
                    }
                }
            }
        }

        user.Status = request.Model.Status;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach (var error in result.Errors)
            {
                string fieldName = IdentityHelper.GetFieldForIdentityError(error.Code);
                failures.Add(new ValidationFailure(fieldName, error.Description));
            }
            throw new ValidationException(failures);
        }

        return new ChangeStatusByManagerResponse()
        {
            Message = $"User status changed to {request.Model.Status} successfully.",
        };
    }
}
