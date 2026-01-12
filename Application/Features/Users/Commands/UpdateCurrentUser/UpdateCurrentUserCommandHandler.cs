using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<UpdateCurrentUserCommand, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        UpdateCurrentUserCommand request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId!);

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();


        if(!string.IsNullOrWhiteSpace(request.Model.FullName))
        {
            user.FullName = request.Model.FullName;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.Gender))
        {
            user.Gender = request.Model.Gender;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.PhoneNumber))
        {
            user.PhoneNumber = request.Model.PhoneNumber;
        }

        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }

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
