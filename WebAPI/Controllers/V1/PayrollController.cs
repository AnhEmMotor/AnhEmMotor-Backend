using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetPayrollSummary;
using Asp.Versioning;
using Domain.Constants.Permission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using Infrastructure.Authorization.Attribute;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý bảng lương")]
[Route("api/v{version:apiVersion}/hr/payroll")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class PayrollController(ISender mediator) : ApiController
{
    [HttpGet("summary")]
    [RequiresAnyPermissions(Permissions.Admin.PayrollManagement.View, Permissions.Accountant.PayrollManagement.View)]
    [ProducesResponseType(typeof(List<PayrollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] int month, [FromQuery] int year, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPayrollSummaryQuery(month, year), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("{id:int}/approve")]
    [RequiresAnyPermissions(Permissions.Admin.PayrollManagement.Configure, Permissions.Accountant.PayrollManagement.Configure)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ApprovePayrollCommand(id, DateTime.UtcNow.Month, DateTime.UtcNow.Year), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
