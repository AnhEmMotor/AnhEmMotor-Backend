using Application.Features.Technologies.Commands.CreateTechnology;
using Application.Features.Technologies.Commands.CreateTechnologyCategory;
using Application.Features.Technologies.Queries.GetAllTechnologies;
using Application.Features.Technologies.Queries.GetAllTechnologyCategories;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing technologies and technology categories.
/// </summary>
/// <param name="mediator">The mediator instance.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TechnologiesController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Gets all technologies, optionally filtered by category or brand.
    /// </summary>
    /// <param name="category_id">The category ID filter.</param>
    /// <param name="brand_id">The brand ID filter.</param>
    /// <returns>A list of technologies.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? category_id, [FromQuery] int? brand_id)
    {
        return HandleResult(await mediator.Send(new GetAllTechnologiesQuery(category_id, brand_id)));
    }

    /// <summary>
    /// Gets all technology categories.
    /// </summary>
    /// <returns>A list of technology categories.</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        return HandleResult(await mediator.Send(new GetAllTechnologyCategoriesQuery()));
    }

    /// <summary>
    /// Creates a new technology.
    /// </summary>
    /// <param name="command">The create technology command.</param>
    /// <returns>The result of the creation.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTechnologyCommand command)
    {
        return HandleResult(await mediator.Send(command));
    }

    /// <summary>
    /// Creates a new technology category.
    /// </summary>
    /// <param name="command">The create technology category command.</param>
    /// <returns>The result of the creation.</returns>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateTechnologyCategoryCommand command)
    {
        return HandleResult(await mediator.Send(command));
    }
}
