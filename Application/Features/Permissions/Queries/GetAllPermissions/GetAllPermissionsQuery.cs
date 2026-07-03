using Application.Common.Models;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<Result<List<PermissionModuleMetadata>>>;
