using Application.Features.Loyalty.Queries.GetLoyaltyMembers;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý Chăm sóc khách hàng &amp; Hội viên (Loyalty)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Chăm sóc khách hàng & Hội viên (Loyalty)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LoyaltyController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách hội viên và điểm thưởng
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp của Sieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Danh sách hội viên</returns>
    [HttpGet("members")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách hội viên và điểm thưởng")]
    public async Task<IActionResult> GetMembersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLoyaltyMembersQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }
}
