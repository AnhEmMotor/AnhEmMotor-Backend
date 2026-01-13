
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed record UpdateManyProductStatusesCommand : IRequest<Result<List<int>?>>
{
    public List<int>? Ids { get; init; }

    public string? StatusId { get; init; }
}
