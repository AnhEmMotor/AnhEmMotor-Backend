using Application.ApiContracts.Product.Responses;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteManyProducts;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.RestoreManyProducts;
using Application.Features.Products.Commands.RestoreProduct;
using Application.Features.Products.Commands.UpdateManyProductPrices;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
using Application.Features.Products.Commands.UpdateManyVariantPrices;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateProductPrice;
using Application.Features.Products.Commands.UpdateProductStatus;
using Application.Features.Products.Commands.UpdateVariantPrice;
using Application.Features.Products.Queries.CheckSlugAvailability;
using Application.Features.Products.Queries.GetActiveVariantLiteListForOutput;
using Application.Features.Products.Queries.GetDeletedProductsList;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProductsList;
using Application.Features.Products.Queries.GetVariantLiteByProductId;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý sản phẩm.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý sản phẩm")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ dành cho khách hàng (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] SieveModel request, CancellationToken cancellationToken)
    {
        var query = GetProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đầy đủ dành cho người quản lý (có phân trang, lọc, tìm kiếm).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("for-manager")]
    [HasPermission(Products.View)]
    [ProducesResponseType(
        typeof(Domain.Primitives.PagedResult<ProductDetailForManagerResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsForManager(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = Application.Features.Products.Queries.GetProductsListForManager.GetProductsListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm đã bị xoá (có phân trang, lọc, tìm kiếm).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Products.View)]
    [ProducesResponseType(
        typeof(Domain.Primitives.PagedResult<ProductDetailForManagerResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedProducts(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken)
    {
        var query = GetDeletedProductsListQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền xem danh sách sản phẩm).
    /// </summary>
    [HttpGet("variants-lite/for-manager")]
    [RequiresAnyPermissions(Products.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProducts(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForManager.GetActiveVariantLiteListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - cho phép vào với mọi user).
    /// </summary>
    [HttpGet("variants-lite")]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsPublic(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForManager.GetActiveVariantLiteListForManagerQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// `Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền thêm hoặc sửa phiếu nhập).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("variants-lite/for-input")]
    [RequiresAnyPermissions(Inputs.Edit, Inputs.Create)]
    [ProducesResponseType(
        typeof(Domain.Primitives.PagedResult<ProductVariantLiteResponseForInput>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsForInput(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = Application.Features.Products.Queries.GetActiveVariantLiteListForInput.GetActiveVariantLiteListForInputQuery
            .FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách biến thể sản phẩm của tất cả sản phẩm (có phân trang, lọc, tìm kiếm - chỉ có thể vào khi và chỉ
    /// khi có quyền thêm hoặc sửa phiếu bán hàng).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("variants-lite/for-output")]
    [RequiresAnyPermissions(Outputs.Edit, Outputs.Create)]
    [ProducesResponseType(
        typeof(Domain.Primitives.PagedResult<ProductVariantLiteResponseForOutput>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVariantLiteProductsForOutput(
        [FromQuery] SieveModel request,
        CancellationToken cancellationToken = default)
    {
        var query = GetActiveVariantLiteListForOutputQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id (dành cho toàn bộ người dùng khách)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVarientById(int id, CancellationToken cancellationToken = default)
    {
        var (data, error) = await sender.Send(new GetProductByIdQuery(id), cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy thông tin chi tiết sản phẩm theo Id (dành cho người quản lý)
    /// </summary>
    [HttpGet("{id:int}/for-manager")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVarientByIdForManager(int id, CancellationToken cancellationToken = default)
    {
        var (data, error) = await sender.Send(new GetProductByIdQuery(id), cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách biến thể theo Id sản phẩm.
    /// </summary>
    [HttpGet("{id:int}/variants-lite")]
    [HasPermission(Products.View)]
    [ProducesResponseType(typeof(List<ProductVariantLiteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVariantLiteByProductIdActive(
        int id,
        CancellationToken cancellationToken = default)
    {
        var (variants, error) = await sender.Send(
            new GetVariantLiteByProductIdQuery(id, IncludeDeleted: false),
            cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(variants);
    }

    /// <summary>
    /// Kiểm tra slug có sẵn sàng sử dụng hay không.
    /// </summary>
    [HttpGet("check-slug")]
    [RequiresAnyPermissions(Products.Create, Products.Edit)]
    [ProducesResponseType(typeof(SlugAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlugAvailability([FromQuery] string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CheckSlugAvailabilityQuery(slug), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Tạo mới sản phẩm với các biến thể.
    /// </summary>
    [HttpPost]
    [HasPermission(Products.Create)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] Application.ApiContracts.Product.Requests.CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateProductCommand>();
        var (data, error) = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return StatusCode(StatusCodes.Status201Created, data);
    }

    /// <summary>
    /// Cập nhật sản phẩm theo Id.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Products.Edit)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] Application.ApiContracts.Product.Requests.UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductCommand(id, request), cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Xoá sản phẩm (soft delete) và cascade xoá ảnh của các biến thể.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var error = await sender.Send(new DeleteProductCommand(id), cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Xoá nhiều sản phẩm cùng lúc (soft delete) và cascade xoá ảnh.
    /// </summary>
    [HttpDelete("delete-many")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProducts(
        [FromBody] Application.ApiContracts.Product.Requests.DeleteManyProductsRequest request,
        CancellationToken cancellationToken)
    {
        var error = await sender.Send(new DeleteManyProductsCommand(request.Ids!), cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return NoContent();
    }

    /// <summary>
    /// Khôi phục sản phẩm đã bị xoá.
    /// </summary>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreProduct(int id, CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new RestoreProductCommand(id), cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPost("restore-many")]
    [HasPermission(Products.Delete)]
    [ProducesResponseType(typeof(List<ProductDetailForManagerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreProducts(
        [FromBody] Application.ApiContracts.Product.Requests.RestoreManyProductsRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new RestoreManyProductsCommand(request.Ids!), cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho tất cả các biến thể của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/price")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductPrice(
        int id,
        [FromBody] Application.ApiContracts.Product.Requests.UpdateProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductPriceCommand(id, request.Price ?? 0), cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("prices")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductPrices(
        [FromBody] Application.ApiContracts.Product.Requests.UpdateManyProductPricesRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(
            new UpdateManyProductPricesCommand(request.Ids!, request.Price),
            cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho 1 biến thể sản phẩm.
    /// </summary>
    [HasPermission(Products.EditPrice)]
    [HttpPatch("variant/{variantId:int}/price")]
    [ProducesResponseType(typeof(ProductVariantLiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariantPrice(
        int variantId,
        [FromBody] Application.ApiContracts.Product.Requests.UpdateVariantPriceRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(
            new UpdateVariantPriceCommand(variantId, request.Price ?? 0),
            cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh giá cho nhiều biến thể sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("variant/prices")]
    [HasPermission(Products.EditPrice)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyVariantPrices(
        [FromBody] Application.ApiContracts.Product.Requests.UpdateManyVariantPricesRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(
            new UpdateManyVariantPricesCommand(request.Ids!, request.Price),
            cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh trạng thái của 1 sản phẩm.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Products.ChangeStatus)]
    [ProducesResponseType(typeof(ProductDetailForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStatus(
        int id,
        [FromBody] Application.ApiContracts.Product.Requests.UpdateProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(new UpdateProductStatusCommand(id, request.StatusId!), cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }

        return Ok(data);
    }

    /// <summary>
    /// Chỉnh trạng thái nhiều sản phẩm cùng lúc.
    /// </summary>
    [HttpPatch("statuses")]
    [HasPermission(Products.ChangeStatus)]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyProductStatuses(
        [FromBody] Application.ApiContracts.Product.Requests.UpdateManyProductStatusesRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await sender.Send(
            new UpdateManyProductStatusesCommand(request.Ids!, request.StatusId!),
            cancellationToken)
            .ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }

        return Ok(data);
    }
}