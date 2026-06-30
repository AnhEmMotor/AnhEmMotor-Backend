using Application.Features.HR.Queries.GetEmployeeKPIs;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using Application.Common.Models;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý KPI nhân viên")]
[Route("api/v{version:apiVersion}/hr/kpis")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class EmployeeKPIController(IMediator mediator) : ApiController
{
    [HttpGet]
    [HasPermission(HR.View)]
    [ProducesResponseType(typeof(List<KpiResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeKPIsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
