using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangePasswordByManager;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.CreateUserByManager;
using Application.Features.UserManager.Commands.DeleteUserByManager;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Commands.UploadAvatarForAdmin;
using Application.Features.UserManager.Queries.GetUserById;
using Application.Features.UserManager.Queries.GetUsersList;
using Application.Features.UserManager.Queries.GetUsersListForOutput;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây).
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
    /// Tạo người dùng mới (Admin).
    /// </summary>
    /// <param name="model">Thông tin người dùng mới (họ tên, email, mật khẩu, roles).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost]
    [HasPermission(Permissions.Admin.UserManagement.Create)]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUserAsync(
        [FromBody] CreateUserByManagerCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(model, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng (có phân trang, lọc, sắp xếp — Sieve).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    [RequiresAnyPermissions(Permissions.Admin.UserManagement.View)]
    [ProducesResponseType(typeof(PagedResult<UserDTOForManagerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin cơ bản của tất cả người dùng (Id, FullName, Email, PhoneNumber) để dùng trong các form nhập liệu.
    /// Dành cho nhân viên có quyền tạo hoặc sửa phiếu bán hàng hoặc xem danh sách người dùng.
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("for-output")]
    [RequiresAnyPermissions(
        Permissions.Order.OrderManagement.Edit,
        Permissions.Order.OrderManagement.Create,
        Permissions.Admin.UserManagement.View)]
    [ProducesResponseType(typeof(PagedResult<UserDTOForOutputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsersForOutputAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersListForOutputQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID.
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần xem.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("{userId:guid}")]
    [HasPermission(Permissions.Admin.UserManagement.View)]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery() { UserId = userId }, cancellationToken)
        .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin người dùng.
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần cập nhật.</param>
    /// <param name="model">Thông tin mới của người dùng.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPut("{userId:guid}")]
    [HasPermission(Permissions.Admin.UserManagement.Edit)]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserAsync(
        Guid userId,
        [FromBody] UpdateUserCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đổi mật khẩu người dùng theo ID (đang đăng nhập).
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần đổi mật khẩu.</param>
    /// <param name="model">Thông tin mật khẩu mới.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{userId:guid}/change-password")]
    [HasPermission(Permissions.Admin.UserManagement.ChangePassword)]
    [ProducesResponseType(typeof(ChangePasswordByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePasswordAsync(
        Guid userId,
        [FromBody] ChangePasswordByManagerCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Gán roles cho người dùng (thêm/xóa vai trò).
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần gán role.</param>
    /// <param name="model">Danh sách role IDs cần gán.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{userId:guid}/assign-roles")]
    [ProducesResponseType(typeof(AssignRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Permissions.Admin.UserManagement.AssignRoles)]
    public async Task<IActionResult> AssignRolesAsync(
        Guid userId,
        [FromBody] AssignRolesCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Thay đổi trạng thái (active/inactive) của một người dùng.
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần đổi trạng thái.</param>
    /// <param name="model">Trạng thái mới (isActive: true/false).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPatch("{userId:guid}/status")]
    [HasPermission(Permissions.Admin.UserManagement.Edit)]
    [ProducesResponseType(typeof(ChangeStatusUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserStatusAsync(
        Guid userId,
        [FromBody] ChangeUserStatusCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = userId };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Thay đổi trạng thái của nhiều người dùng cùng lúc (batch update).
    /// </summary>
    /// <param name="model">Danh sách userIds và trạng thái mới cần áp dụng.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(ChangeStatusMultiUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(Permissions.Admin.UserManagement.Edit)]
    public async Task<IActionResult> ChangeMultipleUsersStatusAsync(
        [FromBody] ChangeMultipleUsersStatusCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
        new ChangeMultipleUsersStatusCommand()
        {
            Status = model.Status,
            UserIds = model.UserIds,
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        },
        cancellationToken)
        .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa người dùng (soft delete).
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần xóa.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("{userId:guid}")]
    [HasPermission(Permissions.Admin.UserManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserId = string.IsNullOrEmpty(currentUserIdStr)
        ? Guid.Empty
        : Guid.Parse(currentUserIdStr);
        var command = new DeleteUserByManagerCommand(userId, currentUserId);
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Tải lên ảnh đại diện cho người dùng theo Id (Admin).
    /// </summary>
    /// <param name="userId">ID (Guid) của người dùng cần upload avatar.</param>
    /// <param name="file">File ảnh đại diện.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{userId:guid}/avatar")]
    [HasPermission(Permissions.Admin.UserManagement.Edit)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminUploadAvatarAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        var command = new UploadAvatarForAdminCommand
        {
            UserId = userId,
            FileContent = file.OpenReadStream(),
            FileName = file.FileName
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
