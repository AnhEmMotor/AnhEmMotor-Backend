using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.News.Commands.UpdateNewsStatus;

public sealed record UpdateNewsStatusCommand : IRequest<Result<Unit>>
{
    public int Id { get; init; }

    [JsonPropertyName("is_published")]
    public bool IsPublished { get; init; }
}
