using Application.ApiContracts.User.Responses;
using Application.Interfaces.Repositories.Authentication;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed class GetUsersListQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUsersListQuery, Domain.Primitives.PagedResult<UserResponse>>
{
    public Task<Domain.Primitives.PagedResult<UserResponse>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    { return userRepository.GetPagedListAsync(request.SieveModel, cancellationToken); }
}