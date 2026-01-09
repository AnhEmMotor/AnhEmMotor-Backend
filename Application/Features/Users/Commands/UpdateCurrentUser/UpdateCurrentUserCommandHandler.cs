using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<UpdateCurrentUserCommand, UserDTOForManagerResponse>
{
    public async Task<UserDTOForManagerResponse> Handle(
        UpdateCurrentUserCommand request,
        CancellationToken cancellationToken)
    {
        if(!GenderStatus.IsValid(request.Model.Gender))
        {
            throw new ValidationException([ new ValidationFailure("gender", "Invalid gender. Please check again.") ]);
        }

        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

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
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
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
