using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.UserManager.Commands.CreateUserByManager;

public class CreateUserByManagerCommandHandler(
    IUserCreateRepository userCreateRepository,
    IUserReadRepository userReadRepository) : IRequestHandler<CreateUserByManagerCommand, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        CreateUserByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = string.IsNullOrEmpty(request.Username) ? request.Email : request.Username,
            Email = request.Email,
            FullName = string.IsNullOrEmpty(request.FullName!) ? request.Email! : request.FullName,
            PhoneNumber = request.PhoneNumber,
            Gender = request.Gender ?? GenderStatus.Male,
            Status = request.Status ?? UserStatus.Active
        };
        var (succeeded, errors) = await userCreateRepository.CreateUserAsync(user, request.Password!, cancellationToken)
            .ConfigureAwait(false);
        if (!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<UserDTOForManagerResponse>.Failure(validationErrors);
        }
        if (request.RoleNames != null && request.RoleNames.Count > 0)
        {
            foreach (var role in request.RoleNames)
            {
                await userCreateRepository.AddUserToRoleAsync(user, role, cancellationToken).ConfigureAwait(false);
            }
        }
        var newUser = await userReadRepository.FindUserByIdAsync(user.Id, cancellationToken).ConfigureAwait(false);
        var roles = await userReadRepository.GetUserRoleIdsAsync(newUser!, cancellationToken).ConfigureAwait(false);
        return new UserDTOForManagerResponse()
        {
            Id = newUser!.Id,
            UserName = newUser.UserName,
            Email = newUser.Email,
            FullName = newUser.FullName,
            Gender = newUser.Gender,
            PhoneNumber = newUser.PhoneNumber,
            EmailConfirmed = newUser.EmailConfirmed,
            Status = newUser.Status,
            AvatarUrl = newUser.AvatarUrl,
            DateOfBirth = newUser.DateOfBirth,
            DeletedAt = newUser.DeletedAt,
            Roles = roles
        };
    }
}
