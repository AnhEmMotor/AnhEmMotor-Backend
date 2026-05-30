using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreBrand;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.ExportBrands;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrandsList;
using Application.Features.Brands.Queries.GetBrandStatistics;
using Application.Features.Brands.Queries.GetDeletedBrandsList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách các thương hiệu sản phẩm (ví dụ: Honda, Yamaha, Suzuki).
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh sách các thương hiệu sản phẩm")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BrandController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp - dành cho mọi người dùng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thương hiệu.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu cho quản lý (có quyền xem danh sách).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thương hiệu dành cho quản lý.</returns>
    [HttpGet("for-manager")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandsForManagerAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách thương hiệu đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thương hiệu đã xóa.</returns>
    [HttpGet("deleted")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedBrandsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedBrandsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê chung về các thương hiệu sản phẩm.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin thống kê thương hiệu.</returns>
    [HttpGet("statistics")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(BrandStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetBrandStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xuất danh sách thương hiệu ra file Excel (có hỗ trợ lọc và sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>File Excel chứa danh sách thương hiệu.</returns>
    [HttpGet("export")]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportBrandsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new ExportBrandsQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của thương hiệu được chọn.
    /// </summary>
    /// <param name="id">ID của thương hiệu.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết thương hiệu.</returns>
    [HttpGet("{id:int}", Name = Domain.Constants.RouteNames.Brands.GetById)]
    [HasPermission(Brands.View)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetBrandByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo thương hiệu mới.
    /// </summary>
    /// <param name="request">Thông tin thương hiệu cần tạo.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thương hiệu vừa được tạo.</returns>
    [HttpPost]
    [HasPermission(Brands.Create)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBrandAsync(
        [FromBody] CreateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateBrandCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(
            result,
            Domain.Constants.RouteNames.Brands.GetById,
            new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    /// <summary>
    /// Cập nhật thông tin thương hiệu.
    /// </summary>
    /// <param name="id">ID thương hiệu cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin thương hiệu sau cập nhật.</returns>
    [HttpPut("{id:int}")]
    [HasPermission(Brands.Edit)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrandAsync(
        int id,
        [FromBody] UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateBrandCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa thương hiệu.
    /// </summary>
    /// <param name="id">ID thương hiệu cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa.</returns>
    [HttpDelete("{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrandAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteBrandCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục thương hiệu đã xóa.
    /// </summary>
    /// <param name="id">ID thương hiệu cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin thương hiệu sau khi khôi phục.</returns>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreBrandAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreBrandCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều thương hiệu cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách ID thương hiệu cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa nhiều.</returns>
    [HttpDelete("delete-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBrandsAsync(
        [FromBody] DeleteManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyBrandsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều thương hiệu đã xóa cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách ID thương hiệu cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách thương hiệu sau khi khôi phục.</returns>
    [HttpPost("restore-many")]
    [HasPermission(Brands.Delete)]
    [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreBrandsAsync(
        [FromBody] RestoreManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyBrandsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}