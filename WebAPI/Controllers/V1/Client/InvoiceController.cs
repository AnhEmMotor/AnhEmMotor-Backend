using Application.Features.Client.Invoices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1.Client;

[ApiController]
[Route("api/v1/client/invoices")]
[Authorize]
public class InvoiceController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetMyInvoices(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.Identity?.Name
            ?? string.Empty;

        var userId = Guid.Parse(userIdStr);

        var result = await mediator.Send(new GetMyInvoicesQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceDetail(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvoiceDetailQuery(id), cancellationToken);
        return result != null ? Ok(result) : NotFound();
    }
}
