using Application.ApiContracts.Permission.Requests;
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
    /// Mô tả của vai trò (tuỳ chọn)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Danh sách quyền cho vai trò (bắt buộc - phải có ít nhất 1 quyền)
    /// </summary>
    public List<string>? Permissions { get; init; }
}
