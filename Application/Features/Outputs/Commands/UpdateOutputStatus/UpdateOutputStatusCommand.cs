using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed record UpdateOutputStatusCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;

    public List<int>? SelectedVehicleIds { get; init; }

    [JsonIgnore]
    public Guid? CurrentUserId { get; init; }
}
