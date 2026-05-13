using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.UpdateVehicleType;

public sealed record UpdateVehicleTypeCommand : IRequest<Result<Unit>>
{
    public int ProductId { get; init; }

    [JsonPropertyName("vehicle_type_id")]
    public int VehicleTypeId { get; init; }
}

