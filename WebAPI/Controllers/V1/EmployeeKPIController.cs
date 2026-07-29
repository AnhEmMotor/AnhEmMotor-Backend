using Application.Common.Models;
using Application.Features.HR.Commands.CreateEmployeeKpi;
using Application.Features.HR.Commands.DeleteEmployeeKpi;
using Application.Features.HR.Commands.UpdateEmployeeKpi;
using Application.Features.HR.Queries.GetEmployeeKPIs;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý chỉ số KPI của nhân viên.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý KPI nhân viên")]
[Route("api/v{version:apiVersion}/hr/kpis")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class EmployeeKPIController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách KPI của tất cả nhân viên.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách chỉ số KPI của từng nhân viên.</returns>
    [HttpGet]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.View, Permissions.Accountant.EmployeeManagement.View)]
    [ProducesResponseType(typeof(List<KpiResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeKPIsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo KPI mới cho nhân viên.
    /// </summary>
    [HttpPost]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeKpiCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result);
    }

    /// <summary>
    /// Cập nhật KPI của nhân viên.
    /// </summary>
    [HttpPut("{id:int}")]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.Edit)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEmployeeKpiCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa KPI của nhân viên.
    /// </summary>
    [HttpDelete("{id:int}")]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.Delete)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteEmployeeKpiCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
