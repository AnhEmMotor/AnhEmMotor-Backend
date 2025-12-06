using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public class DeleteCurrentUserAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IRequestHandler<DeleteCurrentUserAccountCommand, DeleteUserByUserReponse>
{
    public async Task<DeleteUserByUserReponse> Handle(DeleteCurrentUserAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        // Check if already deleted
        if (user.DeletedAt is not null)
        {
            throw new ValidationException([new ValidationFailure("DeletedAt", "This account has already been deleted.")]);
        }

        // Kiểm tra protected users
        var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers").Get<List<string>>() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            throw new ValidationException([new ValidationFailure("Email", "Protected users cannot delete their account.")]);
        }

        user.DeletedAt = DateTimeOffset.UtcNow;
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

        return new DeleteUserByUserReponse()
        {
            Message = "Your account has been deleted successfully.",
        };
    }
}
