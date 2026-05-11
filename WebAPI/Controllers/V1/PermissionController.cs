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
using Domain.Constants;
using Domain.Constants.Permission;
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
/// Controller qu?n lý quy?n h?n và vai trò
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller qu?n lý quy?n h?n và vai trò")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PermissionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y t?t c? các permissions có trong h? th?ng v?i mô t?
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y c?u trúc quy?n h?n (Nhóm, Xung d?t, Ph? thu?c)
    /// </summary>
    [HttpGet("structure")]
    [HasPermission(PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(PermissionStructureResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionStructureAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionStructureQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y các quy?n c?a ngu?i dùng hi?n t?i
    /// </summary>
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
    /// L?y các quy?n c?a m?t ngu?i dùng theo User ID
    /// </summary>
    [HttpGet("users/{userId:guid}/permissions")]
    [HasPermission(PermissionsList.Users.View)]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissionsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPermissionsByIdQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y các quy?n c?a m?t vai trò c? th?
    /// </summary>
    [HttpGet("roles/{roleId:guid}/permissions", Name = RouteNames.Permission.GetRolePermissions)]
    [HasPermission(PermissionsList.Roles.View)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery() { RoleId = roleId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t quy?n cho m?t vai trò
    /// </summary>
    [HttpPut("roles/{roleId:guid}")]
    [HasPermission(PermissionsList.Roles.Edit)]
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
    /// L?y t?t c? các vai trò (có phân trang, l?c, s?p x?p)
    /// </summary>
    [HttpGet("roles")]
    [HasPermission(PermissionsList.Roles.View)]
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
    /// T?o vai trò m?i v?i các quy?n du?c gán
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(PermissionsList.Roles.Create)]
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
            RouteNames.Permission.GetRolePermissions,
            new { roleId = result.IsSuccess ? result.Value.RoleId : null });
    }

    /// <summary>
    /// Xóa vai trò
    /// </summary>
    [HttpDelete("roles/{roleId:guid}")]
    [HasPermission(PermissionsList.Roles.Delete)]
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
    /// Xóa nhi?u vai trò
    /// </summary>
    [HttpPost("roles/delete-multiple")]
    [HasPermission(PermissionsList.Roles.Delete)]
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
