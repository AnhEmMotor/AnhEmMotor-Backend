using Application.ApiContracts.InventoryLedger.Responses;
using Application.Common.Models;
using Application.Features.InventoryLedgers.Queries.GetInventoryLedger;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Sổ cái tồn kho
    /// </summary>
    [SwaggerTag("Sổ cái tồn kho")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class InventoryLedgersController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Lấy danh sách sổ cái tồn kho
        /// </summary>
        [HttpGet]
        [HasPermission(InventoryReceipts.View)]
        [ProducesResponseType(typeof(List<InventoryLedgerResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInventoryLedgerAsync(
            [FromQuery] GetInventoryLedgerQuery query,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
