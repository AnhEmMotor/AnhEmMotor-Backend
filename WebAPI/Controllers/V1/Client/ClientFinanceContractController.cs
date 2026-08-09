using Application.Interfaces.Repositories.SalesContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Hợp đồng mua xe của khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/finance-contracts")]
[Authorize]
public class ClientFinanceContractController(ISalesContractReadRepository repository) : ApiController
{
    /// <summary>
    /// Lấy danh sách hợp đồng mua xe của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyFinanceContracts(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.Identity?.Name ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }

        var contracts = await repository.GetByCustomerIdAsync(userId, cancellationToken);

        var result = contracts.Select(c => new
        {
            c.Id,
            c.ContractNumber,
            c.CustomerFullName,
            c.CustomerPhone,
            c.VehicleModel,
            c.VehicleVersion,
            c.VehicleColor,
            c.FrameNumber,
            c.EngineNumber,
            c.ActualSalePrice,
            c.DepositAmount,
            c.RemainingAmount,
            c.WarrantyPeriod,
            c.WarrantyScope,
            c.Status,
            c.ShowroomName,
            c.ShowroomAddress,
            c.ShowroomRepresentative,
            SignedDate = c.SignedDate.HasValue
                ? c.SignedDate.Value.ToString("dd/MM/yyyy")
                : null,
            FinalPaymentDeadline = c.FinalPaymentDeadline.HasValue
                ? c.FinalPaymentDeadline.Value.ToString("dd/MM/yyyy")
                : null,
        }).ToList();

        return Ok(result);
    }
}
