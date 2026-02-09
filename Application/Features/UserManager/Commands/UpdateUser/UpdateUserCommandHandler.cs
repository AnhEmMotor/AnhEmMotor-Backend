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
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if(!string.IsNullOrWhiteSpace(request.Gender))
        {
            var gender = request.Gender.Trim();
            if(!GenderStatus.IsValid(gender))
            {
                return Error.Validation(
                    $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}",
                    "Gender");
            }
            user.Gender = gender;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if(!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneNumber = request.PhoneNumber.Trim();
            if(string.Compare(phoneNumber, user.PhoneNumber) != 0)
            {
                var existingUser = await userReadRepository.FindUserByPhoneNumberAsync(phoneNumber, cancellationToken)
                    .ConfigureAwait(false);
                if(existingUser != null && existingUser.Id != user.Id)
                {
                    return Error.Conflict($"Phone number '{phoneNumber}' is already taken.", "PhoneNumber");
                }
                user.PhoneNumber = phoneNumber;
            }
        }

        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if(!succeeded)
        {
            var errorList = errors.ToList();
            if(errorList.Any(
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

        var roles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

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
            DeletedAt = user.DeletedAt,
            Roles = roles
        };
    }
}

