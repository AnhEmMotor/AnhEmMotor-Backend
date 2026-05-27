using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Commands.UpdateRole;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetAllRoles;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetPermissionStructure;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý quyền hạn và vai trò trong hệ thống.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý quyền hạn và vai trò")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PermissionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy tất cả các quyền (permissions) có trong hệ thống kèm mô tả.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tất cả các quyền.</returns>
    [HttpGet("permissions")]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy cấu trúc quyền hạn (Nhóm, Xung đột, Phụ thuộc).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Cấu trúc quyền hạn của hệ thống.</returns>
    [HttpGet("structure")]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(PermissionStructureResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionStructureAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionStructureQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các quyền của người dùng hiện tại.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách quyền và vai trò của bản thân.</returns>
    [HttpGet("my-permissions")]
    [Authorize]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPermissionsAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new GetMyPermissionsQuery() { UserId = userIdClaim }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các quyền của một người dùng cụ thể theo ID.
    /// </summary>
    /// <param name="userId">ID người dùng cần tra cứu.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách quyền và vai trò của người dùng.</returns>
    [HttpGet("users/{userId:guid}/permissions")]
    [HasPermission(Users.View)]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissionsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPermissionsByIdQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các quyền của một vai trò cụ thể.
    /// </summary>
    /// <param name="roleId">ID vai trò.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách tên các quyền được gán cho vai trò.</returns>
    [HttpGet("roles/{roleId:guid}/permissions", Name = Permission.GetRolePermissions)]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery() { RoleId = roleId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin và danh sách quyền cho một vai trò.
    /// </summary>
    /// <param name="roleId">ID vai trò cần cập nhật.</param>
    /// <param name="model">Thông tin cập nhật (Tên, Mô tả, Danh sách quyền).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật vai trò.</returns>
    [HttpPut("roles/{roleId:guid}")]
    [HasPermission(Roles.Edit)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PermissionRoleUpdateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRoleAsync(
        Guid roleId,
        [FromBody] UpdateRoleCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateRoleCommand()
            {
                RoleId = roleId,
                RoleName = model.RoleName,
                Description = model.Description,
                Permissions = model.Permissions
            },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy tất cả các vai trò có trong hệ thống (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách vai trò.</returns>
    [HttpGet("roles")]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(PagedResult<RoleSelectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRolesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllRolesQuery
            {
                Page = sieveModel.Page,
                PageSize = sieveModel.PageSize,
                Filters = sieveModel.Filters,
                Sorts = sieveModel.Sorts
            },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo vai trò mới kèm các quyền được gán.
    /// </summary>
    /// <param name="model">Thông tin vai trò mới (Tên, Mô tả, Danh sách quyền).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin vai trò vừa được tạo.</returns>
    [HttpPost("roles")]
    [HasPermission(Roles.Create)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleCreateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRoleAsync(
        [FromBody] CreateRoleCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateRoleCommand()
            {
                Description = model.Description,
                Permissions = model.Permissions,
                RoleName = model.RoleName
            },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleCreated(
            result,
            Permission.GetRolePermissions,
            new { roleId = result.IsSuccess ? result.Value.RoleId : null });
    }

    /// <summary>
    /// Xóa một vai trò khỏi hệ thống.
    /// </summary>
    /// <param name="roleId">ID vai trò cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa vai trò.</returns>
    [HttpDelete("roles/{roleId:guid}")]
    [HasPermission(Roles.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoleCommand() { RoleId = roleId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều vai trò cùng lúc theo tên.
    /// </summary>
    /// <param name="roleNames">Danh sách tên các vai trò cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa nhiều vai trò.</returns>
    [HttpPost("roles/delete-multiple")]
    [HasPermission(Roles.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMultipleRolesAsync(
        [FromBody] List<string> roleNames,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteMultipleRolesCommand() { RoleNames = roleNames }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }
}