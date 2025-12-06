using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.UserManager.Queries.GetUserById;

public class GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");
        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new UserResponse()
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
