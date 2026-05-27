using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IUserStreamService userStreamService) : IRequestHandler<UpdateUserCommand, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }
        if (request.FullName is not null)
        {
            user.FullName = request.FullName.Trim();
        }
        if (request.Gender is not null)
        {
            var gender = request.Gender.Trim();
            if (!string.IsNullOrEmpty(gender) && !GenderStatus.IsValid(gender))
            {
                return Error.Validation(
                    $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}",
                    "Gender");
            }
            user.Gender = gender;
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (request.PhoneNumber is not null)
        {
            var trimmedPhone = request.PhoneNumber.Trim();
            if (string.IsNullOrEmpty(trimmedPhone))
            {
                user.PhoneNumber = null;
            } else if (string.Compare(trimmedPhone, user.PhoneNumber) != 0)
            {
                var existingUser = await userReadRepository.FindUserByPhoneNumberAsync(trimmedPhone, cancellationToken)
                    .ConfigureAwait(false);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return Error.Conflict($"Phone number '{trimmedPhone}' is already taken.", "PhoneNumber");
                }
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
            var errorList = errors.ToList();
            if (errorList.Any(
                e => e.Contains("taken", StringComparison.OrdinalIgnoreCase) ||
                    e.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
            {
                var conflictErrors = errorList
                    .Where(
                        e => e.Contains("taken", StringComparison.OrdinalIgnoreCase) ||
                            e.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                    .Select(e => Error.Conflict(e))
                    .ToList();
                return Result<UserDTOForManagerResponse>.Failure(conflictErrors);
            }
            var validationErrors = errorList.Select(e => Error.Validation(e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }
        userStreamService.NotifyUserUpdate(user.Id);
        var roles = await userReadRepository.GetUserRoleIdsAsync(user, cancellationToken).ConfigureAwait(false);
        return new UserDTOForManagerResponse
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

