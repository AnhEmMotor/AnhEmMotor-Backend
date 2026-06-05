using Application.ApiContracts.Quotation.Requests;
using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Features.Quotations.Queries.GetApprovedPricesForVariant;
using Application.ApiContracts.PurchaseRequest.Responses;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý giá nhà cung cấp đã được duyệt.
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý giá nhà cung cấp đã được duyệt")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class QuotationsController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Lấy danh sách các giá báo giá đã được approve của biến thể và màu sắc đó.
        /// </summary>
        /// <param name="variantId">ID biến thể sản phẩm.</param>
        /// <param name="colorId">ID màu sắc biến thể (không bắt buộc).</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Danh sách báo giá đã được phê duyệt.</returns>
        [HttpGet("approved-prices")]
        [RequiresAnyPermissions(InventoryReceipts.Create, InventoryReceipts.Edit, Products.View, Products.Edit)]
        [ProducesResponseType(typeof(List<PurchaseRequestQuotedPriceResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApprovedPricesAsync(
            [FromQuery] int variantId,
            [FromQuery] int? colorId,
            CancellationToken cancellationToken)
        {
            var query = new GetApprovedPricesForVariantQuery(variantId, colorId);
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lưu hoặc cập nhật giá nhà cung cấp cho biến thể (không cần Quotation ID).
        /// </summary>
        [HttpPost("approved-prices")]
        [RequiresAnyPermissions(Products.Edit, Quotations.Edit)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveApprovedPriceAsync(
            [FromBody] Application.Features.Quotations.Commands.SaveSupplierPrice.SaveSupplierPriceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa giá nhà cung cấp của biến thể.
        /// </summary>
        [HttpDelete("approved-prices")]
        [RequiresAnyPermissions(Products.Edit, Quotations.Edit)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteApprovedPriceAsync(
            [FromQuery] int variantId,
            [FromQuery] int? colorId,
            [FromQuery] int supplierId,
            CancellationToken cancellationToken)
        {
            var command = new Application.Features.Quotations.Commands.DeleteSupplierPrice.DeleteSupplierPriceCommand(variantId, colorId, supplierId);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
