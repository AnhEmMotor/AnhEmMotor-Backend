using Application.ApiContracts.PurchaseOrder.Requests;
using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Features.PurchaseOrders.Commands.ApproveRejectPurchaseOrder;
using Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using Application.Features.PurchaseOrders.Commands.DeletePurchaseOrder;
using Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;
using Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;
using Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
using Application.Features.PurchaseOrders.Queries.GetPurchaseOrderForInputById;
using Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using Application.Features.PurchaseOrders.Queries.GetPurchaseOrderStatusList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý Đơn chốt mua (Purchase Order).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý đơn chốt mua (PO)")]
    [Route("api/v{version:apiVersion}/purchase-orders")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class PurchaseOrderController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Tạo mới một đơn chốt mua.
        /// </summary>
        [HttpPost]
        [HasPermission(InventoryReceipts.Create)]
        [ProducesResponseType(typeof(PurchaseOrderDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreatePurchaseOrderCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật đơn chốt mua.
        /// </summary>
        [HttpPut("{id:int}")]
        [HasPermission(InventoryReceipts.Edit)]
        [ProducesResponseType(typeof(PurchaseOrderDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            int id,
            [FromBody] UpdatePurchaseOrderCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new UpdatePurchaseOrderCommand
                {
                    Id = id,
                    PurchaseRequestId = command.PurchaseRequestId,
                    SupplierId = command.SupplierId,
                    Note = command.Note,
                    Items = command.Items
                },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa đơn chốt mua.
        /// </summary>
        [HttpDelete("{id:int}")]
        [HasPermission(InventoryReceipts.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeletePurchaseOrderCommand(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Gửi đơn chốt mua (Draft -> Sent).
        /// </summary>
        [HttpPost("{id:int}/send")]
        [HasPermission(InventoryReceipts.Send)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SendPurchaseOrderCommand(id), cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Phê duyệt hoặc từ chối đơn chốt mua (Sent -> Approved/Rejected).
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [HasPermission(InventoryReceipts.ApproveReject)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRejectAsync(
            int id,
            [FromBody] ApproveRejectPurchaseOrderRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new ApproveRejectPurchaseOrderCommand(id, request.Status),
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách các trạng thái của đơn chốt mua.
        /// </summary>
        [HttpGet("status")]
        [RequiresAnyPermissions(InventoryReceipts.View, InventoryReceipts.Create, InventoryReceipts.Edit)]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseOrderStatusesAsync(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseOrderStatusListQuery(), cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách đơn chốt mua (có phân trang, tìm kiếm, lọc).
        /// </summary>
        [HttpGet]
        [HasPermission(InventoryReceipts.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseOrderListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseOrdersQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết đơn chốt mua theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [HasPermission(InventoryReceipts.View)]
        [ProducesResponseType(typeof(PurchaseOrderDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseOrderByIdQuery(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết đơn chốt mua cho Phiếu Nhập Kho.
        /// </summary>
        [HttpGet("{id:int}/for-input")]
        [RequiresAnyPermissions(InventoryReceipts.Create, InventoryReceipts.Edit)]
        [ProducesResponseType(typeof(PurchaseOrderDetailForInputResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForInputByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseOrderForInputByIdQuery(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
