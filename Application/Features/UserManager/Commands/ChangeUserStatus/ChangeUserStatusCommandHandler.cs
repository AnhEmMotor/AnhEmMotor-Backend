using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
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

        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false) ??
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
            var userRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

            foreach(var userRole in userRoles)
            {
                if(superRoles.Contains(userRole))
                {
                    var usersInRole = await userReadRepository.GetUsersInRoleAsync(userRole, cancellationToken).ConfigureAwait(false);
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
        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
        }

        return new ChangeStatusUserByManagerResponse()
        {
            Message = $"User status changed to {request.Model.Status} successfully.",
        };
    }
}
