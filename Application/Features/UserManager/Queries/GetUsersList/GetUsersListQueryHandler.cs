using Application.ApiContracts.User;
using Application.Interfaces.Repositories;
using Domain.Shared;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed class GetUsersListQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersListQuery, PagedResult<UserResponse>>
{
    public async Task<PagedResult<UserResponse>> Handle(
        GetUsersListQuery request,
        CancellationToken cancellationToken)
    {
        return await userRepository.GetPagedListAsync(request.SieveModel, cancellationToken);
    }
}