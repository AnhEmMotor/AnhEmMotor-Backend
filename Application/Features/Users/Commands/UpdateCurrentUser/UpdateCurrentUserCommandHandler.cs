using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
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
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }
        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }
        if (user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }
        if (string.Compare(user.Status, UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (request.FullName is not null)
        {
            user.FullName = request.FullName.Trim();
        }
        if (request.Gender is not null)
        {
            user.Gender = request.Gender.Trim();
        }
        if (request.PhoneNumber is not null)
        {
            var trimmedPhone = request.PhoneNumber.Trim();
            if (string.IsNullOrEmpty(trimmedPhone))
            {
                user.PhoneNumber = null;
            } else if (string.Compare(trimmedPhone, user.PhoneNumber) != 0)
            {
                user.PhoneNumber = trimmedPhone;
            }
        }
        if (request.DateOfBirth.HasValue)
        {
            user.DateOfBirth = request.DateOfBirth.Value;
        }
        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if (!succeeded)
        {
            if (!errors.Any())
            {
                return Error.Validation("Failed to update user.", "UpdateFailed");
            }
            var validationErrors = errors.Select(e => Error.Validation("UpdateError", e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }
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
            AvatarUrl = user.AvatarUrl,
            DateOfBirth = user.DateOfBirth,
            DeletedAt = user.DeletedAt,
            Roles = roles
        };
    }
}

