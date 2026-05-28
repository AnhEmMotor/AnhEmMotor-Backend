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
using Application.Features.PurchaseRequests.Queries.GetQuotedPricesForPR;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý Yêu cầu mua hàng (Purchase Request).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý Yêu cầu mua hàng (PR)")]
    [Route("api/v{version:apiVersion}/purchase-requests")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class PurchaseRequestsController(IMediator mediator, IAuthorizationService authorizationService) : ApiController
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = Guid.TryParse(userIdClaim, out var guid) ? guid : null;

            var result = await mediator.Send(
                new CreatePurchaseRequestCommand
                {
                    Note = command.Note,
                    Items = command.Items,
                    CurrentUserId = currentUserId
                },
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = Guid.TryParse(userIdClaim, out var guid) ? guid : null;

            var authResult = await authorizationService.AuthorizeAsync(User, PurchaseRequests.ApproveReject)
                .ConfigureAwait(true);

            var result = await mediator.Send(
                new UpdatePurchaseRequestCommand
                {
                    Id = id,
                    Note = command.Note,
                    Items = command.Items,
                    CurrentUserId = currentUserId,
                    HasApproveRejectPermission = authResult.Succeeded
                },
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
            var authResult = await authorizationService.AuthorizeAsync(User, PurchaseRequests.ApproveReject)
                .ConfigureAwait(true);

            var result = await mediator.Send(new DeletePurchaseRequestCommand(id, authResult.Succeeded), cancellationToken)
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
            var result = await mediator.Send(new SendPurchaseRequestCommand(id), cancellationToken)
                .ConfigureAwait(true);

            return HandleResult(result);
        }

        /// <summary>
        /// Phê duyệt yêu cầu mua hàng (Sent -> Approved).
        /// </summary>
        [HttpPost("{id:int}/approve")]
        [HasPermission(PurchaseRequests.ApproveReject)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveAsync(int id, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = Guid.TryParse(userIdClaim, out var guid) ? guid : null;

            var result = await mediator.Send(new ApproveRejectPurchaseRequestCommand(id, true, currentUserId), cancellationToken)
                .ConfigureAwait(true);

            return HandleResult(result);
        }

        /// <summary>
        /// Từ chối yêu cầu mua hàng (Sent -> Rejected).
        /// </summary>
        [HttpPost("{id:int}/reject")]
        [HasPermission(PurchaseRequests.ApproveReject)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectAsync(int id, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? currentUserId = Guid.TryParse(userIdClaim, out var guid) ? guid : null;

            var result = await mediator.Send(new ApproveRejectPurchaseRequestCommand(id, false, currentUserId), cancellationToken)
                .ConfigureAwait(true);

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
            var result = await mediator.Send(new GetPurchaseRequestsQuery { SieveModel = sieveModel }, cancellationToken)
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

        /// <summary>
        /// Lấy danh sách các giá báo giá hợp lệ cho các mặt hàng trong PR.
        /// </summary>
        [HttpGet("{id:int}/quoted-prices")]
        [HasPermission(PurchaseRequests.View)]
        [ProducesResponseType(typeof(List<PurchaseRequestQuotedPriceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuotedPricesAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetQuotedPricesForPRQuery(id), cancellationToken)
                .ConfigureAwait(true);

            return HandleResult(result);
        }
    }
}
