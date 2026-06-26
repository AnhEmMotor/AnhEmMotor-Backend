using Application.ApiContracts.DebtPayment.Requests;
using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Features.DebtPayments.Commands.PaySupplierDebt;
using Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId;
using Application.Features.DebtPayments.Queries.GetSupplierDebtLogs;
using Application.Features.DebtPayments.Queries.GetSuppliersWithDebt;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý công nợ nhà cung cấp
    /// </summary>
    [SwaggerTag("Quản lý công nợ nhà cung cấp")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class DebtPaymentsController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Lấy danh sách các nhà cung cấp đang còn nợ
        /// </summary>
        [HttpGet("suppliers")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(PagedResult<SupplierDebtResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuppliersWithDebtAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = new GetSuppliersWithDebtQuery { SieveModel = sieveModel };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách các phiếu nhập hàng còn nợ của nhà cung cấp
        /// </summary>
        [HttpGet("suppliers/{supplierId:int}/receipts")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(List<InventoryReceiptDebtLineResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReceiptsWithDebtBySupplierIdAsync(
            [FromRoute] int supplierId,
            CancellationToken cancellationToken)
        {
            var query = new GetReceiptsWithDebtBySupplierIdQuery { SupplierId = supplierId };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Thanh toán nợ cho nhà cung cấp (tự động cấn trừ từ cũ đến mới)
        /// </summary>
        [HttpPost("suppliers/{supplierId:int}/pay")]
        [HasPermission(DebtPayments.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PaySupplierDebtAsync(
            [FromRoute] int supplierId,
            [FromBody] PaySupplierDebtRequest request,
            CancellationToken cancellationToken)
        {
            var command = new PaySupplierDebtCommand { SupplierId = supplierId, Amount = request.Amount, ProofImageUrls = request.ProofImageUrls };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy lịch sử thanh toán nợ của nhà cung cấp
        /// </summary>
        [HttpGet("suppliers/{supplierId:int}/debt-logs")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(List<Application.ApiContracts.DebtPayment.Responses.SupplierDebtLogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSupplierDebtLogsAsync(
            [FromRoute] int supplierId,
            CancellationToken cancellationToken)
        {
            var query = new GetSupplierDebtLogsQuery { SupplierId = supplierId };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Tải lên ảnh chứng minh thanh toán
        /// </summary>
        [HttpPost("proof-image")]
        [HasPermission(DebtPayments.Create)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Application.ApiContracts.DebtPayment.Responses.UploadDebtProofImageResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadDebtProofImageAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var command = new Application.Features.DebtPayments.Commands.UploadDebtProofImage.UploadDebtProofImageCommand
            {
                FileContent = file.OpenReadStream(),
                FileName = file.FileName
            };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xem ảnh chứng minh thanh toán
        /// </summary>
        [HttpGet("proof-image/{mediaFileId:int}")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ViewDebtProofImageAsync(
            [FromRoute] int mediaFileId,
            CancellationToken cancellationToken)
        {
            var query = new Application.Features.DebtPayments.Queries.ViewDebtProofImage.ViewDebtProofImageQuery { MediaFileId = mediaFileId };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);

            if (result.IsFailure)
            {
                return HandleResult(result);
            }

            return File(result.Value.Content, result.Value.ContentType);
        }

        /// <summary>
        /// Lấy danh sách các lần thanh toán thiếu ảnh chứng minh
        /// </summary>
        [HttpGet("missing-proofs")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(PagedResult<Application.ApiContracts.DebtPayment.Responses.SupplierDebtLogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDebtLogsMissingProofsAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = new Application.Features.DebtPayments.Queries.GetDebtLogsMissingProofs.GetDebtLogsMissingProofsQuery { SieveModel = sieveModel };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
        [HttpGet("debt-logs/{id}/proof-images")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDebtLogProofImagesAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new Application.Features.DebtPayments.Queries.GetDebtLogProofImages.GetDebtLogProofImagesQuery { DebtLogId = id }, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPut("debt-logs/{id}/proof-images")]
        [HasPermission(DebtPayments.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDebtProofImagesAsync(
            int id,
            [FromBody] Application.ApiContracts.DebtPayment.Requests.UpdateDebtProofImagesRequest request,
            CancellationToken cancellationToken)
        {
            var command = new Application.Features.DebtPayments.Commands.UpdateDebtProofImages.UpdateDebtProofImagesCommand 
            { 
                DebtLogId = id, 
                ProofImageUrls = request.ProofImageUrls 
            };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
