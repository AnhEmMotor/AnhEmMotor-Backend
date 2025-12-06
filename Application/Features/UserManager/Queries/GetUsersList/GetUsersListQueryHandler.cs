using Application.ApiContracts.User.Responses;
using Application.Interfaces.Repositories.Authentication;
using Domain.Shared;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed class GetUsersListQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUsersListQuery, PagedResult<UserResponse>>
{
    public Task<PagedResult<UserResponse>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    { return userRepository.GetPagedListAsync(request.SieveModel, cancellationToken); }
}