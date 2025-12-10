using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Queries.GetUserById;
using Application.Features.UserManager.Queries.GetUsersList;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
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
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class UserManagerController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách tất cả người dùng (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [RequiresAnyPermissions(Users.View, Outputs.Edit, Outputs.Create)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetUsersListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(Users.View)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    [HttpPut("{userId:guid}")]
    [HasPermission(Users.Edit)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(
        Guid userId,
        [FromBody] UpdateUserRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateUserCommand(userId, model), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Đổi mật khẩu người dùng theo ID
    /// </summary>
    [HttpPost("{userId:guid}/change-password")]
    [HasPermission(Users.ChangePassword)]
    [ProducesResponseType(typeof(ChangePasswordByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        Guid userId,
        [FromBody] ChangePasswordRequest model,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new ChangePasswordCommand(userId, model, currentUserId), cancellationToken)
            .ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Gán roles cho người dùng
    /// </summary>
    [HttpPost("{userId:guid}/assign-roles")]
    [ProducesResponseType(typeof(AssignRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Users.AssignRoles)]
    public async Task<IActionResult> AssignRoles(
        Guid userId,
        [FromBody] AssignRolesRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignRolesCommand(userId, model), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Thay đổi trạng thái của người dùng
    /// </summary>
    [HttpPatch("{userId:guid}/status")]
    [HasPermission(Users.Edit)]
    [ProducesResponseType(typeof(ChangeStatusUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserStatus(
        Guid userId,
        [FromBody] ChangeUserStatusRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ChangeUserStatusCommand(userId, model), cancellationToken)
            .ConfigureAwait(true);
        return Ok(result);
    }


    /// <summary>
    /// Thay đổi trạng thái của nhiều người dùng
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(ChangeStatusMultiUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Users.Edit)]
    public async Task<IActionResult> ChangeMultipleUsersStatus(
        [FromBody] ChangeMultipleUsersStatusRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ChangeMultipleUsersStatusCommand(model), cancellationToken)
            .ConfigureAwait(true);
        return Ok(result);
    }
}
