using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.RestoreUserAccount;

public class RestoreUserAccountCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<RestoreUserAccountCommand, RestoreUserResponse>
{
    public async Task<RestoreUserResponse> Handle(RestoreUserAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.DeletedAt is null)
        {
            throw new ValidationException([new ValidationFailure("DeletedAt", "User account is not deleted.")]);
        }

        // Chỉ có thể khôi phục nếu Status là Active
        if (user.Status != UserStatus.Active)
        {
            throw new ValidationException([new ValidationFailure("Status", $"Cannot restore user with status '{user.Status}'. User status must be Active.")]);
        }

        user.DeletedAt = null;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach (var error in result.Errors)
            {
                string fieldName = IdentityHelper.GetFieldForIdentityError(error.Code);
                failures.Add(new ValidationFailure(fieldName, error.Description));
            }
            throw new ValidationException(failures);
        }

        return new RestoreUserResponse()
        {
            Message = "User account has been restored successfully.",
        };
    }
}
