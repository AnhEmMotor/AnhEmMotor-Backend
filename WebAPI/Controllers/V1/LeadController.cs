using Application.ApiContracts.Leads.Responses;
using Application.Features.Leads.Queries.GetLeads;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý Khách hàng tiềm năng (Leads)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Khách hàng tiềm năng (Leads)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LeadController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách Leads
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeadsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLeadsQuery(), cancellationToken);
        return Ok(result);
    }
}
