using Application.Features.Client.Invoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý hóa đơn của khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/invoices")]
[Authorize]
public class InvoiceController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách hóa đơn của khách hàng đang đăng nhập.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    public async Task<IActionResult> GetMyInvoices(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
        User.FindFirst("sub")?.Value ??
        User.Identity?.Name ??
        string.Empty;
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }
        var result = await mediator.Send(new GetMyInvoicesQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một hóa đơn theo ID.
    /// </summary>
    /// <param name="id">ID của hóa đơn.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceDetail(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvoiceDetailQuery(id), cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }
}
