using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        return new UserResponse()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
        };
    }
}
