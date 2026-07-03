using Application.Features.Sales.Returns.Queries.GetReturnRequestDetail;
using Application.Features.Sales.Returns.Queries.GetReturnRequests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1.Sales;

[ApiController]
[Route("api/v1/sales/returns")]
public class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetReturnRequests([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetReturnRequestsQuery { SieveModel = sieveModel };
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReturnRequestDetail(int id, CancellationToken cancellationToken)
    {
        var query = new GetReturnRequestDetailQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }
}
