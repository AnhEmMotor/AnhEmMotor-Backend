using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.UserManager.Commands.UpdateUser;

public class UpdateUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<UpdateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");
        if(!string.IsNullOrWhiteSpace(request.Model.FullName))
        {
            user.FullName = request.Model.FullName;
        }

        if(!string.IsNullOrWhiteSpace(request.Model.Gender))
        {
            if(!GenderStatus.IsValid(request.Model.Gender))
            {
                throw new ValidationException(
                    [ new ValidationFailure(
                        "Gender",
                        $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}") ]);
            }
            user.Gender = request.Model.Gender;
        }

        cancellationToken.ThrowIfCancellationRequested();

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

        return new UserResponse
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
