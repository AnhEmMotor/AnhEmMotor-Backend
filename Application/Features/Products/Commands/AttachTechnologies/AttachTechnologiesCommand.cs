using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.AttachTechnologies;

public sealed record AttachTechnologiesCommand : IRequest<Result<Unit>>
{
    public int ProductId { get; init; }

    [JsonPropertyName("tech_ids")]
    public List<int> TechIds { get; init; } = [];
}

