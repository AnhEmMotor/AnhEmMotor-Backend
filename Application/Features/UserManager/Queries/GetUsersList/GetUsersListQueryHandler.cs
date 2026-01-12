using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Primitives;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed class GetUsersListQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUsersListQuery, Result<PagedResult<UserDTOForManagerResponse>>>
{
    public async Task<Result<PagedResult<UserDTOForManagerResponse>>> Handle(
        GetUsersListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await userReadRepository.GetPagedListAsync(request.SieveModel!, cancellationToken);
        return result;
    }
}