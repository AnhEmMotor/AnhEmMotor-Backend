using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteMultipleRoles;

public record DeleteMultipleRolesCommand(List<string> RoleNames) : IRequest<Result<RoleDeleteResponse>>;
