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
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Qu?n lý quy?n h?n và vai trò trong h? th?ng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý quy?n h?n và vai trò")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PermissionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y t?t c? các quy?n (permissions) có trong h? th?ng kèm mô t?.
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh sách t?t c? các quy?n.</returns>
    [HttpGet("permissions")]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y c?u trúc quy?n h?n (Nhóm, Xung d?t, Ph? thu?c).
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>C?u trúc quy?n h?n c?a h? th?ng.</returns>
    [HttpGet("structure")]
    [HasPermission(Roles.View)]
    [ProducesResponseType(typeof(PermissionStructureResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionStructureAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionStructureQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh sách các quy?n c?a ngu?i dùng hi?n t?i.
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh sách quy?n và vai trò c?a b?n thân.</returns>
    [HttpGet("my-permissions")]
    [Authorize]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyPermissionsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh sách các quy?n c?a m?t ngu?i dùng c? th? theo ID.
    /// </summary>
    /// <param name="userId">ID ngu?i dùng c?n tra c?u.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh sách quy?n và vai trò c?a ngu?i dùng.</returns>
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
    /// L?y danh sách các quy?n c?a m?t vai trò c? th?.
    /// </summary>
    /// <param name="roleId">ID vai trò.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh sách tên các quy?n du?c gán cho vai trò.</returns>
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
    /// C?p nh?t thông tin và danh sách quy?n cho m?t vai trò.
    /// </summary>
    /// <param name="roleId">ID vai trò c?n c?p nh?t.</param>
    /// <param name="model">Thông tin c?p nh?t (Tên, Mô t?, Danh sách quy?n).</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? c?p nh?t vai trò.</returns>
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
    /// L?y t?t c? các vai trò có trong h? th?ng (có phân trang, l?c, s?p x?p).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, l?c, s?p x?p.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
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
    /// T?o vai trò m?i kèm các quy?n du?c gán.
    /// </summary>
    /// <param name="model">Thông tin vai trò m?i (Tên, Mô t?, Danh sách quy?n).</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Thông tin vai trò v?a du?c t?o.</returns>
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
    /// Xóa m?t vai trò kh?i h? th?ng.
    /// </summary>
    /// <param name="roleId">ID vai trò c?n xóa.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? xóa vai trò.</returns>
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
    /// Xóa nhi?u vai trò cùng lúc theo tên.
    /// </summary>
    /// <param name="roleNames">Danh sách tên các vai trò c?n xóa.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? xóa nhi?u vai trò.</returns>
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
