using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.UserManager.Commands.ChangePassword;

public class ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ChangePasswordCommand, ChangePasswordByManagerResponse>
{
    public async Task<ChangePasswordByManagerResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.CurrentUserId) || !Guid.TryParse(request.CurrentUserId, out var currentUserGuid))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        if (currentUserGuid != request.UserId)
        {
            throw new ForbiddenException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        // Check old password != new password
        if (request.Model.CurrentPassword == request.Model.NewPassword)
        {
            throw new ValidationException([new ValidationFailure("NewPassword", "New password must be different from current password.")]);
        }

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

        return new ChangePasswordByManagerResponse() { Message = "Password changed successfully." };
    }
}
