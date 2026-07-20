using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class OutputController(ISender sender) : ApiController
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get Output List")]
    public async Task<IActionResult> GetOutputs([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOutputsListQuery { SieveModel = sieveModel }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get Output Detail")]
    public async Task<IActionResult> GetOutputDetail(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOutputByIdQuery { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create Output")]
    public async Task<IActionResult> CreateOutput(
        [FromBody] CreateOutputCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update Output")]
    public async Task<IActionResult> UpdateOutput(
        int id,
        [FromBody] UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete Output")]
    public async Task<IActionResult> DeleteOutput(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOutputCommand { Id = id }, cancellationToken);
        return HandleResult(result);
    }
}
