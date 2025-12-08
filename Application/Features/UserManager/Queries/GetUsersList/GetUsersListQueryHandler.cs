using Application.ApiContracts.User.Responses;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed class GetUsersListQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUsersListQuery, Domain.Primitives.PagedResult<UserResponse>>
{
    public Task<Domain.Primitives.PagedResult<UserResponse>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    { return userReadRepository.GetPagedListAsync(request.SieveModel, cancellationToken); }
}