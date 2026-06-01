using Application.ApiContracts.PurchaseRequest.Requests;
using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.SendPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequests;
using Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequests;
using Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequestById;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestStatusList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using WebAPI.Controllers.Base;
using Domain.Entities;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý Yêu cầu mua hàng (Purchase Request).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý Yêu cầu mua hàng (PR)")]
    [Route("api/v{version:apiVersion}/purchase-requests")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class PurchaseRequestsController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Tạo mới một yêu cầu mua hàng.
        /// </summary>
        [HttpPost]
        [HasPermission(PurchaseRequests.Create)]
        [ProducesResponseType(typeof(PurchaseRequestDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreatePurchaseRequestCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new CreatePurchaseRequestCommand { Note = command.Note, Items = command.Items, },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật yêu cầu mua hàng.
        /// </summary>
        [HttpPut("{id:int}")]
        [HasPermission(PurchaseRequests.Edit)]
        [ProducesResponseType(typeof(PurchaseRequestDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            int id,
            [FromBody] UpdatePurchaseRequestCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new UpdatePurchaseRequestCommand { Id = id, Note = command.Note, Items = command.Items },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa yêu cầu mua hàng.
        /// </summary>
        [HttpDelete("{id:int}")]
        [HasPermission(PurchaseRequests.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeletePurchaseRequestCommand(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Gửi yêu cầu mua hàng (Draft -> Sent).
        /// </summary>
        [HttpPost("{id:int}/send")]
        [HasPermission(PurchaseRequests.Send)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SendPurchaseRequestCommand(id), cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Phê duyệt hoặc từ chối yêu cầu mua hàng (Sent -> Approved/Rejected).
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [HasPermission(PurchaseRequests.ApproveReject)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRejectAsync(
            int id,
            [FromBody] ApproveRejectPurchaseRequestRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new ApproveRejectPurchaseRequestCommand(id, request.Status),
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách các trạng thái của yêu cầu mua hàng.
        /// </summary>
        [HttpGet("status")]
        [RequiresAnyPermissions(PurchaseRequests.View, PurchaseRequests.Create, PurchaseRequests.Edit)]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseRequestStatusesAsync(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseRequestStatusListQuery(), cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu mua hàng (có phân trang, tìm kiếm, lọc).
        /// </summary>
        [HttpGet]
        [HasPermission(PurchaseRequests.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseRequestListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseRequestsQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu mua hàng đã duyệt (dành cho người có quyền Tạo/Sửa phiếu Purchase Order - phiếu đặt hàng).
        /// </summary>
        [HttpGet("approved")]
        [RequiresAnyPermissions(Domain.Constants.Permission.Permissions.PurchaseOrder.Create, Domain.Constants.Permission.Permissions.PurchaseOrder.Edit)]
        [ProducesResponseType(typeof(PagedResult<PurchaseRequestListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApprovedAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetApprovedPurchaseRequestsQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu mua hàng đã duyệt theo ID.
        /// </summary>
        [HttpGet("approved/{id:int}")]
        [RequiresAnyPermissions(InventoryReceipts.Create, InventoryReceipts.Edit)]
        [ProducesResponseType(typeof(ApprovedPurchaseRequestDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApprovedByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetApprovedPurchaseRequestByIdQuery(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu mua hàng theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [HasPermission(PurchaseRequests.View)]
        [ProducesResponseType(typeof(PurchaseRequestDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseRequestByIdQuery(id), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
