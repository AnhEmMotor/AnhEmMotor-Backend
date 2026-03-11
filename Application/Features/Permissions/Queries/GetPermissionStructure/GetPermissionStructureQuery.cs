using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetPermissionStructure;

public sealed record GetPermissionStructureQuery : IRequest<Result<PermissionStructureResponse>>;
