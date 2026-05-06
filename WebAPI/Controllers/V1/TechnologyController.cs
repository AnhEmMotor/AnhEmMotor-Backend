using Application.ApiContracts.Technology.Responses;
using Application.Features.Technologies.Queries.GetTechnologiesList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các công nghệ.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TechnologyController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách các công nghệ.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TechnologyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnologiesAsync(CancellationToken cancellationToken)
    {
        var query = new GetTechnologiesListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
