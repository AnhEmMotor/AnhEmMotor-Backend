using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Queries.GetUserById;
using Application.Features.UserManager.Queries.GetUsersList;
using Application.Features.UserManager.Queries.GetUsersListForOutput;
using Asp.Versioning;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)
/// </summary>
/// <remarks>
/// This controller enforces business rules to prevent modification or deletion of protected users and roles, such as
/// SuperRoles and users listed in the protected configuration. All endpoints require specific permissions and may
/// return error responses if protection rules are violated. API versioning is supported via the route template. Thread
/// safety is managed by ASP.NET Core's request handling; concurrent requests may result in race conditions if user or
/// role state changes rapidly.
/// </remarks>
/// <param name="mediator">The MediatR mediator used to send queries and commands.</param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UserManagerController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả người dùng (có phân trang, lọc, sắp xếp - chỉ vào được khi người dùng có quyền xem danh
    /// sách người dùng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [RequiresAnyPermissions(Users.View)]
    [ProducesResponseType(typeof(PagedResult<UserDTOForManagerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng (có phân trang, lọc, sắp xếp - chỉ vào được khi người dùng có quyền sửa hoặc xoá
    /// phiếu bán hàng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("for-output")]
    [RequiresAnyPermissions(Outputs.Edit, Outputs.Create)]
    [ProducesResponseType(typeof(PagedResult<UserDTOForManagerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsersForOutputAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersListForOutputQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(Users.View)]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    [HttpPut("{userId:guid}")]
    [HasPermission(Users.Edit)]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserAsync(
        Guid userId,
        [FromBody] UpdateUserCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Đổi mật khẩu người dùng theo ID (đang đăng nhập)
    /// </summary>
    [HttpPost("{userId:guid}/change-password")]
    [HasPermission(Users.ChangePassword)]
    [ProducesResponseType(typeof(ChangePasswordByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePasswordAsync(
        Guid userId,
        [FromBody] Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Gán roles cho người dùng
    /// </summary>
    [HttpPost("{userId:guid}/assign-roles")]
    [ProducesResponseType(typeof(AssignRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Users.AssignRoles)]
    public async Task<IActionResult> AssignRolesAsync(
        Guid userId,
        [FromBody] AssignRolesCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Thay đổi trạng thái của người dùng
    /// </summary>
    [HttpPatch("{userId:guid}/status")]
    [HasPermission(Users.Edit)]
    [ProducesResponseType(typeof(ChangeStatusUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserStatusAsync(
        Guid userId,
        [FromBody] ChangeUserStatusCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }


    /// <summary>
    /// Thay đổi trạng thái của nhiều người dùng
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(ChangeStatusMultiUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Users.Edit)]
    public async Task<IActionResult> ChangeMultipleUsersStatusAsync(
        [FromBody] ChangeMultipleUsersStatusCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangeMultipleUsersStatusCommand() { Status = model.Status, UserIds = model.UserIds, CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }
}
