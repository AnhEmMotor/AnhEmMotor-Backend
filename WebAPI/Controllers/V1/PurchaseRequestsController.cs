using Application.ApiContracts.PurchaseRequest.Requests;
using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.CloneManyPurchaseRequests;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.DeleteManyPurchaseRequests;
using Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.ImportPurchaseRequests;
using Application.Features.PurchaseRequests.Commands.RestoreManyPurchaseRequests;
using Application.Features.PurchaseRequests.Commands.RestorePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.SendPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using Application.Features.PurchaseRequests.Queries.ExportPurchaseRequests;
using Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequestById;
using Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequests;
using Application.Features.PurchaseRequests.Queries.GetDeletedPurchaseRequestsList;
using Application.Features.PurchaseRequests.Queries.GetImportPurchaseRequestTemplate;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestAuditLogs;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequests;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestStatusList;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
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
    public class PurchaseRequestsController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Tạo mới một yêu cầu mua hàng.
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Create)]
        [ProducesResponseType(typeof(PurchaseRequestDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreatePurchaseRequestCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new CreatePurchaseRequestCommand { Note = command.Note, Items = command.Items, },
                cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật yêu cầu mua hàng.
        /// </summary>
        [HttpPut("{id:int}")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Edit)]
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
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa yêu cầu mua hàng.
        /// </summary>
        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeletePurchaseRequestCommand(id), cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Gửi yêu cầu mua hàng (Draft -> Sent).
        /// </summary>
        [HttpPost("{id:int}/send")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Send)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SendPurchaseRequestCommand(id), cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Phê duyệt hoặc từ chối yêu cầu mua hàng (Sent -> Approved/Rejected).
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.ApproveReject)]
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
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách các trạng thái của yêu cầu mua hàng.
        /// </summary>
        [HttpGet("status")]
        [RequiresAnyPermissions(Permissions.Warehouse.PurchaseRequestManagement.View, Permissions.Warehouse.PurchaseRequestManagement.Create, Permissions.Warehouse.PurchaseRequestManagement.Edit)]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseRequestStatusesAsync(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseRequestStatusListQuery(), cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu mua hàng (có phân trang, tìm kiếm, lọc).
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseRequestListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseRequestsQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách yêu cầu mua hàng đã duyệt (dành cho người có quyền Tạo/Sửa phiếu nhập).
        /// </summary>
        [HttpGet("approved")]
        [RequiresAnyPermissions(Permissions.Warehouse.ReceiptManagement.Create, Permissions.Warehouse.ReceiptManagement.Edit)]
        [ProducesResponseType(typeof(PagedResult<PurchaseRequestListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApprovedAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetApprovedPurchaseRequestsQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu mua hàng đã duyệt theo ID.
        /// </summary>
        [HttpGet("approved/{id:int}")]
        [RequiresAnyPermissions(Permissions.Warehouse.ReceiptManagement.Create, Permissions.Warehouse.ReceiptManagement.Edit)]
        [ProducesResponseType(typeof(ApprovedPurchaseRequestDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApprovedByIdAsync(
            int id,
            [FromQuery] int? excludePurchaseOrderId,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetApprovedPurchaseRequestByIdQuery(id, excludePurchaseOrderId),
                cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu mua hàng theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
        [ProducesResponseType(typeof(PurchaseRequestDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetPurchaseRequestByIdQuery(id), cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy lịch sử chỉnh sửa yêu cầu mua hàng.
        /// </summary>
        [HttpGet("{id:int}/audit-logs")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
        [ProducesResponseType(typeof(List<PurchaseRequestAuditLogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPurchaseRequestAuditLogsAsync(int id, CancellationToken cancellationToken)
        {
            var query = new GetPurchaseRequestAuditLogsQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        [HttpDelete("delete-many")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Delete)]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteManyPurchaseRequestsAsync(
            [FromBody] DeleteManyPurchaseRequestsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpPost("restore/{id:int}")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Delete)]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestorePurchaseRequestAsync(int id, CancellationToken cancellationToken)
        {
            var command = new RestorePurchaseRequestCommand { Id = id };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpPost("restore-many")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Delete)]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreManyPurchaseRequestsAsync(
            [FromBody] RestoreManyPurchaseRequestsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpPost("clone-many")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Create)]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CloneManyPurchaseRequestsAsync(
            [FromBody] CloneManyPurchaseRequestsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpPost("import")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Create)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Result<ImportPurchaseRequestsResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportPurchaseRequestsAsync(
            [FromForm] ImportPurchaseRequestsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpGet("import-template")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.Create)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetImportTemplateAsync(CancellationToken cancellationToken)
        {
            var query = new GetImportPurchaseRequestTemplateQuery();
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return File(
                    result.Value,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Mau_nhap_yeu_cau_mua_hang.xlsx");
            }
            return HandleResult(result);
        }

        [HttpGet("export")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportPurchaseRequestsAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = new ExportPurchaseRequestsQuery { SieveModel = sieveModel };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return File(
                result,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Danh_sach_yeu_cau_mua_hang.xlsx");
        }

        [HttpGet("deleted")]
        [HasPermission(Permissions.Warehouse.PurchaseRequestManagement.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseRequestListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedPurchaseRequestsAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = new GetDeletedPurchaseRequestsListQuery { SieveModel = sieveModel };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }
    }
}
