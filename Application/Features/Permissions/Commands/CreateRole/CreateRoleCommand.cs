using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Result<RoleCreateResponse>>
{
    /// <summary>
    /// Tên vai trò
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    /// Mô t? c?a vai trò (tu? ch?n)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Danh sách quy?n cho vai trò (b?t bu?c - ph?i có ít nh?t 1 quy?n)
    /// </summary>
    public List<string>? Permissions { get; init; }
}
