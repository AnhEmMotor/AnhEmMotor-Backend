using Application.Features.Technologies.Commands.CreateTechnology;
using Application.Features.Technologies.Commands.CreateTechnologyCategory;
using Application.Features.Technologies.Queries.GetAllTechnologies;
using Application.Features.Technologies.Queries.GetAllTechnologyCategories;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TechnologiesController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? category_id, [FromQuery] int? brand_id)
    {
        return HandleResult(await mediator.Send(new GetAllTechnologiesQuery(category_id, brand_id)));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        return HandleResult(await mediator.Send(new GetAllTechnologyCategoriesQuery()));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTechnologyCommand command)
    {
        return HandleResult(await mediator.Send(command));
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateTechnologyCategoryCommand command)
    {
        return HandleResult(await mediator.Send(command));
    }
}
