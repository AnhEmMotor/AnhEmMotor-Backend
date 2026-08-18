using Application.Common.Models;
using Application.Features.Client.Outputs.Queries.GetPersonalOutputs;
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
[Route("api/v1/client/outputs")]
[Authorize]
public class ClientOutputController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public ClientOutputController(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("personal")]
    [SwaggerOperation(Summary = "Get personal outputs of the current user")]
    public async Task<IActionResult> GetPersonalOutputs([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserContext.GetUserId();
        var result = await _mediator.Send(new GetPersonalOutputsQuery 
        { 
            CurrentUserId = currentUserId,
            SieveModel = sieveModel 
        }, cancellationToken);
        
        return Ok(Result<object>.Success(result));
    }
}
