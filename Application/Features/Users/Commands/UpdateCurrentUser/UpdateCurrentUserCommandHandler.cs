using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<UpdateCurrentUserCommand, UserDTOForManagerResponse>
{
    public async Task<UserDTOForManagerResponse> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        if(!GenderStatus.IsValid(request.Model.Gender))
        {
            throw new ValidationException([ new ValidationFailure("gender", "Invalid gender. Please check again.") ]);
        }

        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        cancellationToken.ThrowIfCancellationRequested();


        if(!string.IsNullOrWhiteSpace(request.Model.FullName))
        {
            user.FullName = request.Model.FullName;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.Gender))
        {
            user.Gender = request.Model.Gender;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.PhoneNumber))
        {
            user.PhoneNumber = request.Model.PhoneNumber;
        }

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

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserDTOForManagerResponse()
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
