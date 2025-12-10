using Application.ApiContracts.Auth.Responses;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
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

        var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        if(!result.Succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in result.Errors)
            {
                string fieldName = Common.Helper.IdentityHelper.GetFieldForIdentityError(error.Code);
                failures.Add(new ValidationFailure(fieldName, error.Description));
            }
            throw new ValidationException(failures);
        }

        var defaultRoles = protectedEntityManagerService.GetDefaultRolesForNewUsers() ?? [];
        if(defaultRoles.Count > 0)
        {
            var randomRole = defaultRoles[Random.Shared.Next(defaultRoles.Count)];
            await userManager.AddToRoleAsync(user, randomRole).ConfigureAwait(false);
        }

        return new RegistrationSuccessResponse();
    }
}
