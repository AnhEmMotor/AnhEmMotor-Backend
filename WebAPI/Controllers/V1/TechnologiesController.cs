using Application.ApiContracts.Technology.Responses;
using Application.Features.Technologies.Commands.CreateTechnology;
using Application.Features.Technologies.Commands.CreateTechnologyCategory;
using Application.Features.Technologies.Queries.GetAllTechnologies;
using Application.Features.Technologies.Queries.GetAllTechnologyCategories;
using Application.Features.Technologies.Queries.GetTechnologiesList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các công nghệ và loại công nghệ.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TechnologiesController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách các công nghệ (phiên bản rút gọn).
    /// </summary>
    [HttpGet("list")]
    [Authorize]
    [ProducesResponseType(typeof(List<TechnologyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnologiesAsync(CancellationToken cancellationToken)
    {
        var query = new GetTechnologiesListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy tất cả công nghệ, có thể lọc theo loại hoặc thương hiệu.
    /// </summary>
    /// <param name="category_id">ID của loại công nghệ.</param>
    /// <param name="brand_id">ID của thương hiệu.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách các công nghệ.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int? category_id,
        [FromQuery] int? brand_id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await mediator.Send(new GetAllTechnologiesQuery(category_id, brand_id), cancellationToken)
                .ConfigureAwait(true));
    }

    /// <summary>
    /// Lấy danh sách các loại công nghệ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách các loại công nghệ.</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return HandleResult(
            await mediator.Send(new GetAllTechnologyCategoriesQuery(), cancellationToken).ConfigureAwait(true));
    }

    /// <summary>
    /// Tạo mới một công nghệ.
    /// </summary>
    /// <param name="command">Lệnh tạo công nghệ.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo mới.</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTechnologyCommand command,
        CancellationToken cancellationToken)
    {
        return HandleResult(await mediator.Send(command, cancellationToken).ConfigureAwait(true));
    }

    /// <summary>
    /// Tạo mới một loại công nghệ.
    /// </summary>
    /// <param name="command">Lệnh tạo loại công nghệ.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tạo mới.</returns>
    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateTechnologyCategoryCommand command,
        CancellationToken cancellationToken)
    {
        return HandleResult(await mediator.Send(command, cancellationToken).ConfigureAwait(true));
    }
}
