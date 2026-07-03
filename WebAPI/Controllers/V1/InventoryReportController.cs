using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Features.InventoryReports.Queries.ExportInventoryReport;
using Application.Features.InventoryReports.Queries.GetInventoryReportDetail;
using Application.Features.InventoryReports.Queries.GetInventoryReportSummary;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Báo cáo tồn kho
    /// </summary>
    [SwaggerTag("Báo cáo tồn kho")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class InventoryReportController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Báo cáo tổng hợp tồn kho
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
        [ProducesResponseType(typeof(PagedResult<InventoryReportSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInventoryReportSummaryAsync(
            [FromQuery] GetInventoryReportSummaryQuery query,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xuất báo cáo tồn kho ra tệp Excel
        /// </summary>
        [HttpGet("export")]
        [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportInventoryReportAsync(
            [FromQuery] ExportInventoryReportQuery query,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Báo cáo chi tiết giao dịch kho cho một biến thể sản phẩm và màu sắc
        /// </summary>
        [HttpGet("details")]
        [HasPermission(Permissions.Warehouse.ReceiptManagement.View)]
        [ProducesResponseType(typeof(InventoryReportDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventoryReportDetailAsync(
            [FromQuery] int variantId,
            [FromQuery] int? colorId,
            CancellationToken cancellationToken)
        {
            var query = new GetInventoryReportDetailQuery { VariantId = variantId, ColorId = colorId };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
