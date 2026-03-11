using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetPermissionStructure;

public class GetPermissionStructureQueryHandler : IRequestHandler<GetPermissionStructureQuery, Result<PermissionStructureResponse>>
{
    public Task<Result<PermissionStructureResponse>> Handle(GetPermissionStructureQuery request, CancellationToken cancellationToken)
    {
        var response = new PermissionStructureResponse
        {
            Groups = PermissionsList.Groups,
            Conflicts = PermissionsList.Conflicts,
            Dependencies = PermissionsList.Dependencies,
            Metadata = PermissionsList.GetMetadataList()
                .Select(m => new PermissionMetadataResponse
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description
                }).ToList()
        };

        return Task.FromResult(Result<PermissionStructureResponse>.Success(response));
    }
}
