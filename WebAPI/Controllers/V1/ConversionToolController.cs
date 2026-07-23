using Application.ApiContracts.ConversionTools.Responses;
using Application.Common.Models;
using Application.Features.ConversionTools.Commands.CreateConversionTool;
using Application.Features.ConversionTools.Commands.DeleteConversionTool;
using Application.Features.ConversionTools.Commands.UpdateConversionTool;
using Application.Features.ConversionTools.Queries.GetConversionTools;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Công cụ chuyển đổi (Popup, Landing Page) — quản lý nội dung thu hút khách hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Công cụ chuyển đổi (Popup, Landing Page)")]
[Route("api/v{version:apiVersion}/conversion-tools")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ConversionToolController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả công cụ chuyển đổi.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpGet]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(List<ConversionToolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetConversionToolsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới một công cụ chuyển đổi (popup/landing page).
    /// </summary>
    /// <param name="command">Thông tin công cụ chuyển đổi mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPost]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(ConversionToolResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConversionToolCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin công cụ chuyển đổi theo ID.
    /// </summary>
    /// <param name="id">ID của công cụ chuyển đổi.</param>
    /// <param name="command">Thông tin cập nhật.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpPut("{id:int}")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(ConversionToolResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateConversionToolCommand command,
        CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa công cụ chuyển đổi theo ID.
    /// </summary>
    /// <param name="id">ID của công cụ cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    [HttpDelete("{id:int}")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteConversionToolCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
