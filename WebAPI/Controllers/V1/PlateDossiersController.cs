using Application.Features.PlateDossiers.Commands.CreatePlateDossier;
using Application.Features.PlateDossiers.Commands.UpdatePlateDossierStatus;
using Application.Features.PlateDossiers.Queries.GetPlateDossiersList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Controller quản lý hồ sơ biển số xe (Plate Dossiers).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý hồ sơ biển số xe (Plate Dossiers)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class PlateDossiersController(ISender sender) : ApiController
    {
        /// <summary>
        /// Tạo hồ sơ làm biển số xe mới cho đơn hàng bán xe.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            CreatePlateDossierCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách hồ sơ làm biển số xe có phân trang và bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetPlateDossiersListQuery { SieveModel = sieveModel }, cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật trạng thái tiến độ làm biển số xe.
        /// </summary>
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatusAsync(
            UpdatePlateDossierStatusCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }
    }
}
