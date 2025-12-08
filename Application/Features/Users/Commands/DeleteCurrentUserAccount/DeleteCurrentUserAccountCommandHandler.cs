using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public class DeleteCurrentUserAccountCommandHandler(
    UserManager<ApplicationUser> userManager,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteCurrentUserAccountCommand, DeleteUserByUserReponse>
{
    public async Task<DeleteUserByUserReponse> Handle(
        DeleteCurrentUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");


        if(user.DeletedAt is not null)
        {
            throw new ValidationException(
                [ new ValidationFailure("DeletedAt", "This account has already been deleted.") ]);
        }

        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            throw new ValidationException(
                [ new ValidationFailure("Email", "Protected users cannot delete their account.") ]);
        }

        cancellationToken.ThrowIfCancellationRequested();

        user.DeletedAt = DateTimeOffset.UtcNow;
        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);
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

        return new DeleteUserByUserReponse() { Message = "Your account has been deleted successfully.", };
    }
}
