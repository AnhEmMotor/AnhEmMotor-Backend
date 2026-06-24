using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Features.Services.Commands;
using Application.Features.Services.Queries;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý các dịch vụ bảo trì và sửa chữa của cửa hàng.
/// </summary>
public class ServicesController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Tạo mới một dịch vụ trong hệ thống.
    /// </summary>
    /// <param name="command">Thông tin dịch vụ cần tạo.</param>
    /// <param name="cancellationToken">Token hủy bỏ tác vụ.</param>
    /// <returns>Thông tin dịch vụ vừa được tạo thành công.</returns>
    /// <example>
    /// <code>POST /api/v1/services { "name": "Thay nhớt Honda Vision", "description": "Thay nhớt máy và nhớt số chính
    /// hãng", "basePrice": 150000, "estimatedDurationMinutes": 15, "categoryId": 1 }</code>
    /// </example>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceAsync(
        [FromBody] CreateServiceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách dịch vụ có phân trang và lọc.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServicesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetServicesListQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin dịch vụ.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceAsync(
        int id,
        [FromBody] UpdateServiceCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(
                new ErrorResponse
                {
                    Errors =
                        new List<ErrorDetail>
                            {
                                new() { Field = "Id", Message = "ID trong URL không khớp với ID trong body." }
                            }
                });
        }
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
