using Application.Common.Models;
using Application.Features.Client.Repairs.Queries.GetPersonalRepairs;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers.V1.Client;

[ApiController]
[Route("api/v1/client/repairs")]
[Authorize]
public class ClientRepairController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public ClientRepairController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("personal")]
    [SwaggerOperation(Summary = "Get personal repair orders of the current user")]
    public async Task<IActionResult> GetPersonalRepairs([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserContext.GetUserId();
        var result = await _mediator.Send(new GetPersonalRepairsQuery 
        { 
            CurrentUserId = currentUserId,
            SieveModel = sieveModel 
        }, cancellationToken);
        
        return Ok(Result<object>.Success(result));
    }
}
