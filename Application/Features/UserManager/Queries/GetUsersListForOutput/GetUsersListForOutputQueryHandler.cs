using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Primitives;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUsersListForOutput;

public sealed class GetUsersListForOutputQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUsersListForOutputQuery, Result<PagedResult<UserDTOForOutputResponse>>>
{
    public async Task<Result<PagedResult<UserDTOForOutputResponse>>> Handle(
        GetUsersListForOutputQuery request,
        CancellationToken cancellationToken)
    {
        var result = await userReadRepository.GetPagedListForOutputAsync(request.SieveModel!, cancellationToken);
        return result;
    }
}