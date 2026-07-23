using Application.Common.Models;
using Application.Features.Loyalty.Queries.GetLoyaltyMembers;
using Asp.Versioning;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý chăm sóc khách hàng và hội viên (Loyalty) — theo dõi điểm thưởng, hạng thành viên và lịch sử đổi quà.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Chăm sóc khách hàng & Hội viên (Loyalty)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class LoyaltyController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách hội viên và điểm thưởng hiện tại của họ (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách hội viên với điểm thưởng và thông tin hạng thành viên.</returns>
    /// <response code="200">Trả về danh sách hội viên thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("members")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách hội viên và điểm thưởng")]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLoyaltyMembersQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
