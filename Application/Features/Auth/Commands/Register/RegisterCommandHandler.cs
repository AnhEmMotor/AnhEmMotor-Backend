using Application.ApiContracts.Auth.Responses;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(UserManager<ApplicationUser> userManager, IConfiguration configuration) : IRequestHandler<RegisterCommand, RegistrationSuccessResponse>
{
    public async Task<RegistrationSuccessResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = string.IsNullOrEmpty(request.Username) ? request.Email : request.Username,
            Email = request.Email,
            FullName = request.FullName,
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
                string fieldName = IdentityHelper.GetFieldForIdentityError(error.Code);
                failures.Add(new ValidationFailure(fieldName, error.Description));
            }
            throw new ValidationException(failures);
        }

        var defaultRoles = configuration.GetSection("ProtectedAuthorizationEntities:DefaultRolesForNewUsers")
                .Get<List<string>>() ??
            [];
        if(defaultRoles.Count > 0)
        {
            var randomRole = defaultRoles[Random.Shared.Next(defaultRoles.Count)];
            await userManager.AddToRoleAsync(user, randomRole).ConfigureAwait(false);
        }

        return new RegistrationSuccessResponse();
    }
}
