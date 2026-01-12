using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Commands.UpdateRolePermissions;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetAllRoles;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý quyền hạn và vai trò
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller quản lý quyền hạn và vai trò")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PermissionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy tất cả các permissions có trong hệ thống với mô tả
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy các quyền của người dùng hiện tại
    /// </summary>
    [HttpGet("my-permissions")]
    [Authorize]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new GetMyPermissionsQuery() { UserId = userIdClaim }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy các quyền của một người dùng theo User ID
    /// </summary>
    [HttpGet("users/{userId:guid}/permissions")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Users.View)]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissionsById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPermissionsByIdQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy các quyền của một vai trò cụ thể
    /// </summary>
    [HttpGet("roles/{roleName}/permissions")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissions(string roleName, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery() { RoleName = roleName }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật quyền cho một vai trò
    /// </summary>
    [HttpPut("roles/{roleName}")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.AssignPermissions)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PermissionRoleUpdateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRole(
        string roleName,
        [FromBody] UpdateRoleCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateRoleCommand() { RoleName = roleName, Description = model.Description, Permissions = model.Permissions }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy tất cả các vai trò
    /// </summary>
    [HttpGet("roles")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(List<RoleSelectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllRolesQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo vai trò mới với các quyền được gán
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.Create)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleCreateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand model, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateRoleCommand() { Description = model.Description, Permissions = model.Permissions, RoleName = model.RoleName }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa vai trò
    /// </summary>
    [HttpDelete("roles/{roleName}")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.Delete)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRole(string roleName, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoleCommand() { RoleName = roleName }, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều vai trò
    /// </summary>
    [HttpPost("roles/delete-multiple")]
    [HasPermission(Domain.Constants.Permission.PermissionsList.Roles.Delete)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMultipleRoles(
        [FromBody] List<string> roleNames,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteMultipleRolesCommand() { RoleNames = roleNames }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }
}
