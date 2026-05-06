using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<Dictionary<string, List<PermissionResponse>>>>
{
    public Task<Result<Dictionary<string, List<PermissionResponse>>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = PermissionsList.Groups
            .ToDictionary(
                g => g.Key,
                g => g.Value
                    .Select(
                        p =>
                        {
                            var metadata = PermissionsList.GetMetadata(p);
                            return new PermissionResponse()
                            {
                                ID = p,
                                DisplayName = metadata?.DisplayName ?? p.Split('.').Last(),
                                Description = metadata?.Description,
                            };
                        })
                    .ToList());
        return Task.FromResult<Result<Dictionary<string, List<PermissionResponse>>>>(result);
    }
}
