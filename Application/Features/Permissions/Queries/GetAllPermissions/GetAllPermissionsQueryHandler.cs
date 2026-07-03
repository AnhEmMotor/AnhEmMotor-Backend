using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionModuleMetadata>>>
{
    public Task<Result<List<PermissionModuleMetadata>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = PermissionsList.ModulesTree;
        return Task.FromResult<Result<List<PermissionModuleMetadata>>>(result);
    }
}
