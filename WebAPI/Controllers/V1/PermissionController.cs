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
using Domain.Constants.Permission;
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
/// Qu?n l� quy?n h?n v� vai tr� trong h? th?ng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n l� quy?n h?n v� vai tr�")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PermissionController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y t?t c? c�c quy?n (permissions) c� trong h? th?ng k�m m� t?.
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh s�ch t?t c? c�c quy?n.</returns>
    [HttpGet("permissions")]
    [HasPermission(Permissions.Admin.RoleManagement.View)]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y c?u tr�c quy?n h?n (Nh�m, Xung d?t, Ph? thu?c).
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>C?u tr�c quy?n h?n c?a h? th?ng.</returns>
    [HttpGet("structure")]
    [HasPermission(Permissions.Admin.RoleManagement.View)]
    [ProducesResponseType(typeof(PermissionStructureResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionStructureAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPermissionStructureQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch c�c quy?n c?a ngu?i d�ng hi?n t?i.
    /// </summary>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh s�ch quy?n v� vai tr� c?a b?n th�n.</returns>
    [HttpGet("my-permissions")]
    [Authorize]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyPermissionsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch c�c quy?n c?a m?t ngu?i d�ng c? th? theo ID.
    /// </summary>
    /// <param name="userId">ID ngu?i d�ng c?n tra c?u.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh s�ch quy?n v� vai tr� c?a ngu?i d�ng.</returns>
    [HttpGet("users/{userId:guid}/permissions")]
    [HasPermission(Permissions.Admin.UserManagement.View)]
    [ProducesResponseType(typeof(List<PermissionAndRoleOfUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissionsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPermissionsByIdQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch c�c quy?n c?a m?t vai tr� c? th?.
    /// </summary>
    /// <param name="roleId">ID vai tr�.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh s�ch t�n c�c quy?n du?c g�n cho vai tr�.</returns>
    [HttpGet("roles/{roleId:guid}/permissions", Name = Permission.GetRolePermissions)]
    [HasPermission(Permissions.Admin.RoleManagement.View)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery() { RoleId = roleId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t th�ng tin v� danh s�ch quy?n cho m?t vai tr�.
    /// </summary>
    /// <param name="roleId">ID vai tr� c?n c?p nh?t.</param>
    /// <param name="model">Th�ng tin c?p nh?t (T�n, M� t?, Danh s�ch quy?n).</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? c?p nh?t vai tr�.</returns>
    [HttpPut("roles/{roleId:guid}")]
    [HasPermission(Permissions.Admin.RoleManagement.Edit)]
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
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y t?t c? c�c vai tr� c� trong h? th?ng (c� ph�n trang, l?c, s?p x?p).
    /// </summary>
    /// <param name="sieveModel">C�c th�ng tin ph�n trang, l?c, s?p x?p.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Danh s�ch vai tr�.</returns>
    [HttpGet("roles")]
    [HasPermission(Permissions.Admin.RoleManagement.View)]
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
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// T?o vai tr� m?i k�m c�c quy?n du?c g�n.
    /// </summary>
    /// <param name="model">Th�ng tin vai tr� m?i (T�n, M� t?, Danh s�ch quy?n).</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>Th�ng tin vai tr� v?a du?c t?o.</returns>
    [HttpPost("roles")]
    [HasPermission(Permissions.Admin.RoleManagement.Create)]
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
            .ConfigureAwait(false);
        return HandleCreated(
            result,
            Permission.GetRolePermissions,
            new { roleId = result.IsSuccess ? result.Value.RoleId : null });
    }

    /// <summary>
    /// X�a m?t vai tr� kh?i h? th?ng.
    /// </summary>
    /// <param name="roleId">ID vai tr� c?n x�a.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? x�a vai tr�.</returns>
    [HttpDelete("roles/{roleId:guid}")]
    [HasPermission(Permissions.Admin.RoleManagement.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoleCommand() { RoleId = roleId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// X�a nhi?u vai tr� c�ng l�c theo t�n.
    /// </summary>
    /// <param name="roleNames">Danh s�ch t�n c�c vai tr� c?n x�a.</param>
    /// <param name="cancellationToken">Token h?y b?.</param>
    /// <returns>K?t qu? x�a nhi?u vai tr�.</returns>
    [HttpPost("roles/delete-multiple")]
    [HasPermission(Permissions.Admin.RoleManagement.Delete)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RoleDeleteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMultipleRolesAsync(
        [FromBody] List<string> roleNames,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteMultipleRolesCommand() { RoleNames = roleNames }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
