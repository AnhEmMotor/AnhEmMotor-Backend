using Application.ApiContracts.Auth.Responses;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserCreateRepository userCreateRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<RegisterCommand, RegistrationSuccessResponse>
{
    public async Task<RegistrationSuccessResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = string.IsNullOrEmpty(request.Username) ? request.Email : request.Username,
            Email = request.Email,
            FullName = string.IsNullOrEmpty(request.FullName) ? request.Email : request.FullName,
            PhoneNumber = request.PhoneNumber,
            Gender = request.Gender ?? GenderStatus.Male,
            Status = UserStatus.Active
        };

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userCreateRepository.CreateUserAsync(user, request.Password, cancellationToken).ConfigureAwait(false);

        if(!succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
        }

        var defaultRoles = protectedEntityManagerService.GetDefaultRolesForNewUsers() ?? [];
        if(defaultRoles.Count > 0)
        {
            var randomRole = defaultRoles[Random.Shared.Next(defaultRoles.Count)];
            await userCreateRepository.AddUserToRoleAsync(user, randomRole, cancellationToken).ConfigureAwait(false);
        }

        return new RegistrationSuccessResponse();
    }
}
