using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUserById;

public class GetUserByIdQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        var roles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

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
