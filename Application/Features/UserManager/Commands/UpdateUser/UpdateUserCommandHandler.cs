using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<UpdateUserCommand, UserDTOForManagerResponse>
{
    public async Task<UserDTOForManagerResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");
        if(!string.IsNullOrWhiteSpace(request.Model.FullName))
        {
            user.FullName = request.Model.FullName;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.Gender))
        {
            if(!GenderStatus.IsValid(request.Model.Gender))
            {
                throw new ValidationException(
                    [ new ValidationFailure(
                        "Gender",
                        $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}") ]);
            }
            user.Gender = request.Model.Gender;
        }

        cancellationToken.ThrowIfCancellationRequested();

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
