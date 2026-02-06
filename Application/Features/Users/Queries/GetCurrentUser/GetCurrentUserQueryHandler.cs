using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        if (user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }

        if (string.Compare(user.Status, Domain.Constants.UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }

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
