using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.TransferOwnership;

public sealed record TransferOwnershipCommand : IRequest<Result<VehicleResponse?>>
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonPropertyName("new_lead_id")]
    public int NewLeadId { get; init; }
}
