using Application.ApiContracts.Permission.Responses;
using Domain.Constants;
using MediatR;
using System.Reflection;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Dictionary<string, List<PermissionResponse>>>
{
    public Task<Dictionary<string, List<PermissionResponse>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var permissionDefinitions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(
                fieldInfo => new
                {
                    Category = fieldInfo.DeclaringType?.Name ?? "Unknown",
                    Key = fieldInfo.Name,
                    Permission = fieldInfo.GetRawConstantValue() as string
                })
            .Where(p => p.Permission is not null)
            .ToList();

        var result = permissionDefinitions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(
                    p =>
                    {
                        var metadata = PermissionsList.GetMetadata(p.Permission!);
                        return new PermissionResponse()
                        {
                            ID = p.Permission,
                            DisplayName = metadata?.DisplayName ?? p.Key,
                            Description = metadata?.Description,
                        };
                    })
                    .ToList());

        return Task.FromResult(result);
    }
}
