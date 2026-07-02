using Application.Features.Maintenances.Commands.CreateMaintenanceTicket;
using Application.Features.Maintenances.Commands.UpdateOdo;
using Application.Features.Maintenances.Queries.GetMaintenanceTicketsList;
using Application.Features.Maintenances.Queries.GetMaintenanceTicketDetail;
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
    /// Controller quản lý bảo trì (Maintenance).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý phiếu bảo trì (Maintenance)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class MaintenanceController(ISender sender) : ApiController
    {
        /// <summary>
        /// Tạo phiếu bảo trì mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateMaintenanceTicketCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách phiếu bảo trì.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetMaintenanceTicketsListQuery { SieveModel = sieveModel }, cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết phiếu bảo trì theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetailAsync(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetMaintenanceTicketDetailQuery { Id = id }, cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        /// <summary>
        /// Cập nhật ODO của phương tiện.
        /// </summary>
        [HttpPut("{id:int}/odo")]
        public async Task<IActionResult> UpdateOdoAsync(
            [FromRoute] int id,
            [FromBody] UpdateOdoCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }
    }
}
