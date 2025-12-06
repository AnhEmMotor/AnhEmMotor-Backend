using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public class ChangePasswordCurrentUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ChangePasswordCurrentUserCommand, ChangePasswordUserByUserResponse>
{
    public async Task<ChangePasswordUserByUserResponse> Handle(ChangePasswordCurrentUserCommand request, CancellationToken cancellationToken)
    {
        if (string.Compare(request.Model.CurrentPassword, request.Model.NewPassword) == 0)
        {
            throw new ValidationException([new ValidationFailure("newPassword", "New password can not dupplicate current password.")]);
        }

        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("User not found.");
        var result = await userManager.ChangePasswordAsync(user, request.Model.CurrentPassword, request.Model.NewPassword);
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

        return new ChangePasswordUserByUserResponse() { Message = "Password changed successfully." };
    }
}
