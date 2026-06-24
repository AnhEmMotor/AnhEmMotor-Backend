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
            var command = new PaySupplierDebtCommand { SupplierId = supplierId, Amount = request.Amount };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy lịch sử thanh toán nợ của nhà cung cấp
        /// </summary>
        [HttpGet("suppliers/{supplierId:int}/debt-logs")]
        [HasPermission(DebtPayments.View)]
        [ProducesResponseType(typeof(List<SupplierDebtLog>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSupplierDebtLogsAsync(
            [FromRoute] int supplierId,
            CancellationToken cancellationToken)
        {
            var query = new GetSupplierDebtLogsQuery { SupplierId = supplierId };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
