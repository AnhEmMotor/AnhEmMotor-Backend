using Application.ApiContracts.Brand.Responses;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreBrand;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrandsList;
using Application.Features.Brands.Queries.GetDeletedBrandsList;
using Asp.Versioning;
using Domain.Common.Models;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các thương hiệu sản phẩm (ví dụ: Honda, Yamaha, Suzuki).
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh sách các thương hiệu sản phẩm")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BrandController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp - vào được cho mọi người dùng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrands([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp - vào được khi có quyền xem danh sách thương hiệu).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("for-manager")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandsForManager(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu đã bị xoá (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("deleted")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedBrands(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedBrandsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin của thương hiệu được chọn.
    /// </summary>
    /// <param name="id">Mã thương hiệu cần lấy thông tin.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandById(int id, CancellationToken cancellationToken)
    {
        var query = new GetBrandByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Tạo thương hiệu mới.
    /// </summary>
    /// <param name="request">Truyền tên và mô tả cho thương hiệu đó. Cả 2 đều là 1 chuỗi.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission(Brands.Create)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBrand(
        [FromBody] Application.ApiContracts.Brand.Requests.CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateBrandCommand>();
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Cập nhật thông tin thương hiệu.
    /// </summary>
    /// <param name="id">Id thương hiệu cần cập nhật.</param>
    /// <param name="request">Tên thương hiệu và mô tả cho thương hiệu đó, tất cả đều là 1 chuỗi.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [HasPermission(Brands.Edit)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrand(
        int id,
        [FromBody] Application.ApiContracts.Brand.Requests.UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateBrandCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá thương hiệu.
    /// </summary>
    /// <param name="id">Id của thương hiệu cần xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrand(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteBrandCommand() with { Id = id };
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Khôi phục lại thương hiệu đã xoá.
    /// </summary>
    /// <param name="id">Id của thương hiệu cần khôi phục</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreBrand(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreBrandCommand() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá nhiều thương hiệu cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id thương hiệu cần xoá.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("delete-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBrands(
        [FromBody] Application.ApiContracts.Brand.Requests.DeleteManyBrandsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyBrandsCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều thương hiệu đã xoá cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách Id thương hiệu cần khôi phục.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("restore-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreBrands(
        [FromBody] Application.ApiContracts.Brand.Requests.RestoreManyBrandsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyBrandsCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}