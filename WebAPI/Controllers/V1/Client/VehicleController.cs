using System.Security.Claims;
using Application.Features.Vehicles.Commands.CreateVehicleMaintenanceHistory;
using Application.Features.Vehicles.Commands.CreateVehiclePurchaseHistory;
using Application.Features.Vehicles.Commands.CreateVehicleWarrantyHistory;
using Application.Features.Vehicles.Commands.DeleteClientVehicle;
using Application.Features.Vehicles.Commands.RegisterVehicle;
using Application.Features.Vehicles.Commands.UpdateClientVehicle;
using Application.Features.Vehicles.Queries.GetClientVehicle;
using Application.Features.Vehicles.Queries.GetClientVehicleDetail;
using Application.Features.Vehicles.Queries.GetVehicleHistory;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1.Client;

[ApiController]
[Route("api/v1/client/vehicles")]
[Authorize]
public class VehicleController(IMediator mediator, IVehicleReadRepository vehicleReadRepository) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var vehicles = await vehicleReadRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var payload = vehicles
            .Where(v => v.IsActive)
            .Select(v => new
            {
                id = v.Id,
                name = v.ProductVariant?.Product?.Name ?? "Xe của tôi",
                plate = v.LicensePlate,
                vin = v.VinNumber,
                engine = v.EngineNumber,
                color = string.IsNullOrWhiteSpace(v.Color) ? v.ProductVariantColor?.ColorName : v.Color,
                type = v.ProductVariant?.Product?.Name,
                status = v.Status,
                odo = v.CurrentOdo,
                nextService = v.NextMaintenanceDate,
                regDate = v.PurchaseDate,
                purchaseDate = v.PurchaseDate,
                warrantyDate = v.WarrantyDate,
                warrantyUntil = v.WarrantyDate,
            })
            .ToList();

        return Ok(payload);
    }

    [HttpPost("register-odo")]
    public async Task<IActionResult> RegisterOdo([FromBody] RegisterVehicleCommand command, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(command with { UserId = parsedUserId }, cancellationToken)
            .ConfigureAwait(false);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return HandleResult(result);
    }

    [HttpGet("{vehicleId:int}")]
    public async Task<IActionResult> GetVehicleById(int vehicleId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(new GetClientVehicleQuery(vehicleId, parsedUserId), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpGet("{vehicleId:int}/detail")]
    public async Task<IActionResult> GetVehicleDetail(int vehicleId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(new GetClientVehicleDetailQuery(vehicleId, parsedUserId), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPut("{vehicleId:int}")]
    public async Task<IActionResult> UpdateVehicle(int vehicleId, [FromBody] UpdateClientVehicleCommand command, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(command with { VehicleId = vehicleId, UserId = parsedUserId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpDelete("{vehicleId:int}")]
    public async Task<IActionResult> DeleteVehicle(int vehicleId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(new DeleteClientVehicleCommand(vehicleId, parsedUserId), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpGet("{vehicleId:int}/history")]
    public async Task<IActionResult> GetVehicleHistory(int vehicleId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVehicleHistoryQuery(vehicleId), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("{vehicleId:int}/purchase-history")]
    public async Task<IActionResult> AddPurchaseHistory(int vehicleId, [FromBody] CreateVehiclePurchaseHistoryCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { VehicleId = vehicleId }, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("{vehicleId:int}/warranty-history")]
    public async Task<IActionResult> AddWarrantyHistory(int vehicleId, [FromBody] CreateVehicleWarrantyHistoryCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { VehicleId = vehicleId }, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost("{vehicleId:int}/maintenance-history")]
    public async Task<IActionResult> AddMaintenanceHistory(int vehicleId, [FromBody] CreateVehicleMaintenanceHistoryCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { VehicleId = vehicleId }, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
