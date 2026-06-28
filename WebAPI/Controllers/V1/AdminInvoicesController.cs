using Application.ApiContracts.Admin.Invoices;
using Application.Common.Models;
using Application.Features.Admin.Invoices.Commands.CreateAdminInvoice;
using Application.Features.Admin.Invoices.Commands.UpdateAdminInvoice;
using Application.Features.Admin.Invoices.Commands.UpdateInvoiceStatus;
using Application.Features.Admin.Invoices.Queries.GetAdminInvoiceDetail;
using Application.Features.Admin.Invoices.Queries.GetAdminInvoices;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
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
/// Quản lý hóa đơn bán hàng (Admin).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hóa đơn bán hàng")]
[Route("api/v{version:apiVersion}/Admin/invoices")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AdminInvoicesController(IMediator mediator) : ApiController
{
  /// <summary>
  /// Lấy danh sách hóa đơn bán hàng (có phân trang, lọc, sắp xếp).
  /// </summary>
  [HttpGet]
  [HasPermission(Outputs.View)]
  [ProducesResponseType(typeof(PagedResult<AdminInvoiceSummaryResponse>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetInvoicesAsync(
      [FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
  {
    var query = new GetAdminInvoicesQuery(sieveModel);
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy chi tiết một hóa đơn bán hàng.
  /// </summary>
  [HttpGet("{id:int}")]
  [HasPermission(Outputs.View)]
  [ProducesResponseType(typeof(AdminInvoiceDetailResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetInvoiceDetailAsync(int id, CancellationToken cancellationToken)
  {
    var query = new GetAdminInvoiceDetailQuery(id);
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Tạo hóa đơn bán hàng mới.
  /// </summary>
  [HttpPost]
  [HasPermission(Outputs.Create)]
  [ProducesResponseType(typeof(AdminInvoiceDetailResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateInvoiceAsync(
      [FromBody] CreateAdminInvoiceRequest request, CancellationToken cancellationToken)
  {
    var command = new CreateAdminInvoiceCommand(request);
    var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    return HandleCreated(result, nameof(GetInvoiceDetailAsync), new { id = result.IsSuccess ? result.Value?.Id : 0 });
  }

  /// <summary>
  /// Cập nhật thông tin hóa đơn bán hàng.
  /// </summary>
  [HttpPut("{id:int}")]
  [HasPermission(Outputs.Edit)]
  [ProducesResponseType(typeof(AdminInvoiceDetailResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdateInvoiceAsync(
      int id, [FromBody] UpdateAdminInvoiceRequest request, CancellationToken cancellationToken)
  {
    var command = new UpdateAdminInvoiceCommand(id, request);
    var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Cập nhật trạng thái hóa đơn (ví dụ: xác nhận thanh toán).
  /// </summary>
  [HttpPatch("{id:int}/status")]
  [HasPermission(Outputs.ChangeStatus)]
  [ProducesResponseType(typeof(AdminInvoiceDetailResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdateInvoiceStatusAsync(
      int id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken cancellationToken)
  {
    var command = new UpdateInvoiceStatusCommand(id, request);
    var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }
}
