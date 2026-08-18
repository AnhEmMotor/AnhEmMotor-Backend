using Application.Common.Models;
using Application.Features.Client.Vouchers.Queries.GetPersonalVouchers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý voucher dành riêng cho khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/vouchers")]
[Authorize]
[SwaggerTag("Voucher định danh cá nhân")]
public class ClientVoucherController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientVoucherController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lấy danh sách voucher định danh cá nhân của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet("personal")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPersonalVouchers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPersonalVouchersQuery(GetCurrentUserId()), cancellationToken);
        return Ok(new { value = result });
    }

    private System.Guid GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userIdStr) ? System.Guid.Empty : System.Guid.Parse(userIdStr);
    }
}
