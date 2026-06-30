using Application.ApiContracts.ConversionTools.Responses;
using Application.Common.Models;
using Application.Features.ConversionTools.Commands.CreateConversionTool;
using Application.Features.ConversionTools.Commands.DeleteConversionTool;
using Application.Features.ConversionTools.Commands.UpdateConversionTool;
using Application.Features.ConversionTools.Queries.GetConversionTools;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Công cụ chuyển đổi (Popup, Landing Page)")]
[Route("api/v{version:apiVersion}/conversion-tools")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ConversionToolController(IMediator mediator) : ApiController
{
    [HttpGet]
    [HasPermission(Marketing.View)]
    [ProducesResponseType(typeof(List<ConversionToolResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetConversionToolsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Marketing.Edit)]
    [ProducesResponseType(typeof(ConversionToolResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateConversionToolCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Marketing.Edit)]
    [ProducesResponseType(typeof(ConversionToolResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConversionToolCommand command, CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Marketing.Edit)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteConversionToolCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
