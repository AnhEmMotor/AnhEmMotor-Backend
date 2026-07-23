using Application.ApiContracts.Client.Catalog;
using Application.Features.Client.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Danh mục sản phẩm xe máy (Client Portal — public, không yêu cầu đăng nhập).
/// </summary>
[ApiController]
[Route("api/v1/client/catalog")]
public class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lấy danh sách sản phẩm xe máy (có thể tìm kiếm và lọc theo danh mục).
    /// </summary>
    /// <param name="search">Từ khóa tìm kiếm (tên xe, mã xe).</param>
    /// <param name="categoryId">ID danh mục để lọc.</param>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] string search, [FromQuery] int? categoryId)
    {
        var result = await _mediator.Send(new GetProductsQuery(search, categoryId));
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một sản phẩm xe máy theo ID.
    /// </summary>
    /// <param name="id">ID của sản phẩm.</param>
    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        var result = await _mediator.Send(new GetProductDetailQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Gửi yêu cầu tư vấn về sản phẩm.
    /// </summary>
    /// <param name="request">Thông tin yêu cầu tư vấn (tên, SĐT, nội dung).</param>
    [HttpPost("request-consultation")]
    public async Task<IActionResult> RequestConsultation([FromBody] ConsultationRequest request)
    {
        var result = await _mediator.Send(new RequestConsultationCommand(request));
        return result ? Ok() : BadRequest();
    }
}
