using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Interfaces.Repositories.User;
using MediatR;
using Application.Features.UserManager.Queries.GetUsersList;
using Application;
using Application.Features;
using Application.Features.UserManager;
using Application.Features.UserManager.Queries;

namespace Application.Features.UserManager.Queries.GetUsersListForOutput;

public sealed class GetUsersListForOutputQueryHandler(IUserReadRepository userReadRepository) : IRequestHandler<GetUsersListForOutputQuery, Domain.Primitives.PagedResult<UserDTOForOutputResponse>>
{
    public Task<Domain.Primitives.PagedResult<UserDTOForOutputResponse>> Handle(
        GetUsersListForOutputQuery request,
        CancellationToken cancellationToken)
    { return userReadRepository.GetPagedListForOutputAsync(request.SieveModel, cancellationToken); }
}