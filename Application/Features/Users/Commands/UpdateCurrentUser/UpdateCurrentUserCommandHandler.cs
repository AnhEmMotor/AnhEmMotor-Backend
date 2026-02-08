using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IUserStreamService userStreamService) : IRequestHandler<UpdateCurrentUserCommand, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        UpdateCurrentUserCommand request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }

        if(string.Compare(user.Status, Domain.Constants.UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }

        cancellationToken.ThrowIfCancellationRequested();


        if(!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if(!string.IsNullOrWhiteSpace(request.Gender))
        {
            user.Gender = request.Gender;
        }

        if(!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber.Trim();
        }

        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if(!succeeded)
        {
            if(!errors.Any())
            {
                return Error.Validation("Failed to update user.", "UpdateFailed");
            }
            var validationErrors = errors.Select(e => Error.Validation("UpdateError", e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }

        // Notify user about update
        userStreamService.NotifyUserUpdate(user.Id);

        var roles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

        return new UserDTOForManagerResponse()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Status = user.Status,
            DeletedAt = user.DeletedAt,
            Roles = roles
        };
    }
}

