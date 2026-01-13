using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<UpdateUserCommand, Result<UserDTOForManagerResponse>>
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
            user.FullName = request.FullName;
        }

        if(!string.IsNullOrWhiteSpace(request.Gender))
        {
            if(!GenderStatus.IsValid(request.Gender))
            {
                return Error.Validation(
                    $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}",
                    "Gender");
            }
            user.Gender = request.Gender;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if(!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }

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

