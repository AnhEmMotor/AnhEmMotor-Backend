using Application.Features.WarrantyClaims.Commands.CreateWarrantyClaim;
using Application.Features.WarrantyClaims.Commands.UpdateWarrantyClaimStatus;
using Application.Features.WarrantyClaims.Queries.GetWarrantyClaimsList;
using Application.Features.WarrantyClaims.Queries.GetWarrantyClaimDetail;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Controller quản lý hồ sơ bảo hành (Warranty Claims).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý hồ sơ bảo hành (Warranty Claims)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class WarrantyClaimsController(ISender sender) : ApiController
    {
        /// <summary>
        /// Tạo hồ sơ bảo hành mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateWarrantyClaimCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ bảo hành có phân trang và bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWarrantyClaimsListQuery { SieveModel = sieveModel }, cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết hồ sơ bảo hành theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetailAsync(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWarrantyClaimDetailQuery { Id = id }, cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật trạng thái phiếu bảo hành.
        /// </summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatusAsync(
            [FromRoute] int id,
            [FromBody] UpdateWarrantyClaimStatusCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }
    }
}
