using Application.ApiContracts.PurchaseInvoice.Requests;
using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Features.PurchaseInvoices.Commands.ApproveRejectPurchaseInvoice;
using Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;
using Application.Features.PurchaseInvoices.Commands.DeletePurchaseInvoice;
using Application.Features.PurchaseInvoices.Commands.UpdatePurchaseInvoice;
using Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;
using Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý Hóa đơn mua hàng (Purchase Invoice).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý hóa đơn mua hàng")]
    [Route("api/v{version:apiVersion}/purchase-invoices")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class PurchaseInvoicesController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Tạo mới một hóa đơn mua hàng.
        /// </summary>
        [HttpPost]
        [HasPermission(InventoryReceipts.Create)]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreatePurchaseInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật hóa đơn mua hàng.
        /// </summary>
        [HttpPut("{id:int}")]
        [HasPermission(InventoryReceipts.Edit)]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            int id,
            [FromBody] UpdatePurchaseInvoiceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new UpdatePurchaseInvoiceCommand
                {
                    Id = id,
                    InvoiceNumber = command.InvoiceNumber,
                    InvoiceDate = command.InvoiceDate,
                    DueDate = command.DueDate,
                    Note = command.Note,
                    Items = command.Items
                },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa hóa đơn mua hàng (soft delete).
        /// </summary>
        [HttpDelete("{id:int}")]
        [HasPermission(InventoryReceipts.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeletePurchaseInvoiceCommand(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Phê duyệt hoặc từ chối hóa đơn mua hàng (Draft -> Approved/Rejected).
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [HasPermission(InventoryReceipts.ApproveReject)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRejectAsync(
            int id,
            [FromBody] ApproveRejectPurchaseInvoiceRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new ApproveRejectPurchaseInvoiceCommand(id, request.IsApproved, request.Note),
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách hóa đơn mua hàng (có phân trang, tìm kiếm, lọc).
        /// </summary>
        [HttpGet]
        [HasPermission(InventoryReceipts.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseInvoiceListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseInvoicesQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn mua hàng theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [HasPermission(InventoryReceipts.View)]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseInvoiceByIdQuery(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
