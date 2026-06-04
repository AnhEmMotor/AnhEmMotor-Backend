using Application.ApiContracts.DebtPayment.Requests;
using Application.ApiContracts.DebtPayment.Responses;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.DebtPayments.Commands.RecordDebtPayment;
using Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId;
using Application.Features.DebtPayments.Queries.GetSuppliersWithDebt;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        [ProducesResponseType(typeof(List<SupplierDebtResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuppliersWithDebtAsync(CancellationToken cancellationToken)
        {
            var query = new GetSuppliersWithDebtQuery();
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
        /// Thực hiện thanh toán nợ cho dòng chi tiết phiếu nhập hàng
        /// </summary>
        [HttpPost("{lineId:int}/pay")]
        [HasPermission(DebtPayments.Create)]
        [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayDebtAsync(
            [FromRoute] int lineId,
            [FromBody] PayDebtRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RecordDebtPaymentCommand 
            { 
                LineId = lineId, 
                Amount = request.Amount
            };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
