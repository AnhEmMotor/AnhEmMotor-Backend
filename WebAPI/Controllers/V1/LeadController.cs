using Application.ApiContracts.Leads.Responses;
using Application.Features.Leads.Commands.UpdateLead;
using Application.Features.Leads.Queries.GetLeads;
using Application.Features.Leads.Commands.ResetLeads;
using Application.Features.Leads.Commands.AssignLead;
using Application.Features.Leads.Commands.SeedLeads;
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

    /// <summary>
    /// Lấy thông tin chi tiết khách hàng theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeadByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new Application.Features.Leads.Queries.GetLeadById.GetLeadByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách khách hàng theo luồng Pipeline (Kanban)
    /// </summary>
    [HttpGet("pipeline")]
    [Authorize]
    [ProducesResponseType(typeof(List<Application.Features.Leads.Queries.GetLeadPipeline.LeadPipelineGroupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPipelineAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new Application.Features.Leads.Queries.GetLeadPipeline.GetLeadPipelineQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLeadAsync(int id, UpdateLeadCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Thêm ghi chú/lịch sử tương tác
    /// </summary>
    [HttpPost("{id}/activities")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddActivityAsync(int id, [FromBody] Application.Features.Leads.Commands.AddLeadActivity.AddLeadActivityCommand command, CancellationToken cancellationToken)
    {
        if (id != command.LeadId) return BadRequest();
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Giao việc cho nhân viên
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignLeadAsync(int id, [FromBody] Guid? userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignLeadCommand(id, userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo dữ liệu mẫu (NỘI BỘ - KHÔNG AUTH)
    /// </summary>
    [HttpGet("seed-samples")]
    public async Task<IActionResult> SeedSamplesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SeedLeadsCommand(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reset toàn bộ dữ liệu khách hàng (Xóa hết)
    /// </summary>
    [HttpDelete("reset")]
    [Authorize]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetLeadsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResetLeadsCommand(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reset toàn bộ dữ liệu khách hàng (NỘI BỘ - KHÔNG AUTH)
    /// </summary>
    [HttpGet("reset-internal-temporary")]
    public async Task<IActionResult> ResetLeadsInternalAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResetLeadsCommand(), cancellationToken);
        return HandleResult(result);
    }
}
