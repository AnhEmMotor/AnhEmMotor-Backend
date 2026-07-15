using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetPayrollSummary;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý bảng lương.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý bảng lương")]
[Route("api/v{version:apiVersion}/hr/payroll")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PayrollController(ISender mediator) : ApiController
{
    /// <summary>
    /// Lấy tổng hợp bảng lương theo tháng và năm.
    /// </summary>
    /// <param name="month">Tháng cần xem bảng lương (1-12).</param>
    /// <param name="year">Năm cần xem bảng lương.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách bảng lương tổng hợp của tháng/năm chỉ định.</returns>
    [HttpGet("summary")]
    [RequiresAnyPermissions(Permissions.Admin.PayrollManagement.View, Permissions.Accountant.PayrollManagement.View)]
    [ProducesResponseType(typeof(List<PayrollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPayrollSummaryQuery(month, year), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Phê duyệt bảng lương nhân viên theo ID.
    /// </summary>
    /// <param name="id">ID của bảng lương cần phê duyệt.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả phê duyệt (true nếu thành công).</returns>
    [HttpPost("{id:int}/approve")]
    [RequiresAnyPermissions(
        Permissions.Admin.PayrollManagement.Configure,
        Permissions.Accountant.PayrollManagement.Configure)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ApprovePayrollCommand(id, DateTime.UtcNow.Month, DateTime.UtcNow.Year),
            cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
