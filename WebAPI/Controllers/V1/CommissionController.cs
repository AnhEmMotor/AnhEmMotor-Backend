using Application.ApiContracts.HR.Responses;
using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetCommissionRecords;
using Application.Features.HR.Queries.GetPayrollSummary;
using Asp.Versioning;
using Domain.Entities;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý bản ghi hoa hồng của nhân viên.
/// </summary>
/// <param name="mediator">Instance của mediator.</param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý bản ghi hoa hồng của nhân viên")]
[Route("api/v{version:apiVersion}/hr/commissions")]
public class CommissionController(ISender mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả bản ghi hoa hồng.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách các bản ghi hoa hồng.</returns>
    [HttpGet]
    public async Task<IActionResult> GetRecordsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionRecordsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy bảng tổng hợp lương và hoa hồng (payroll).
    /// </summary>
    /// <param name="month">Tháng.</param>
    /// <param name="year">Năm.</param>
    /// <param name="ct">Token hủy bỏ.</param>
    [HttpGet("payroll-summary")]
    [HasPermission(Domain.Constants.Permission.Permissions.Payroll.View)]
    [ProducesResponseType(typeof(List<PayrollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollSummaryAsync(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPayrollSummaryQuery(month, year), ct).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Phê duyệt thanh toán hoa hồng/lương.
    /// </summary>
    /// <param name="command">Lệnh phê duyệt payroll.</param>
    /// <param name="ct">Token hủy bỏ.</param>
    [HttpPost("approve-payroll")]
    [HasPermission(Domain.Constants.Permission.Permissions.Payroll.Approve)]
    public async Task<IActionResult> ApprovePayrollAsync([FromBody] ApprovePayrollCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct).ConfigureAwait(true);
        return HandleResult(result);
    }
}