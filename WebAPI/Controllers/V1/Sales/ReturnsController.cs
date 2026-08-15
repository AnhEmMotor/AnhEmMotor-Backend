using Application.Features.Sales.Returns.Queries.GetReturnRequestDetail;
using Application.Features.Sales.Returns.Queries.GetReturnRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1.Sales;

/// <summary>
/// Quản lý yêu cầu đổi trả hàng bán.
/// </summary>
[ApiController]
[Route("api/v1/sales/returns")]
public class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách yêu cầu đổi trả hàng (có phân trang, lọc, sắp xếp — Sieve).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp của Sieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    public async Task<IActionResult> GetReturnRequests(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetReturnRequestsQuery { SieveModel = sieveModel };
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy chi tiết một yêu cầu đổi trả hàng theo ID.
    /// </summary>
    /// <param name="id">ID của yêu cầu đổi trả.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReturnRequestDetail(int id, CancellationToken cancellationToken)
    {
        var query = new GetReturnRequestDetailQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    /// <summary>
    /// Tạo yêu cầu đổi trả hàng mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReturnRequest(
        [FromBody] Application.Features.Sales.Returns.Commands.CreateReturnRequest.CreateReturnRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý/phê duyệt yêu cầu đổi trả.
    /// </summary>
    [HttpPut("{id}/process")]
    public async Task<IActionResult> ProcessReturnRequest(
        int id,
        [FromBody] Application.Features.Sales.Returns.Commands.ProcessReturnRequest.ProcessReturnRequestCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ReturnRequestId)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
