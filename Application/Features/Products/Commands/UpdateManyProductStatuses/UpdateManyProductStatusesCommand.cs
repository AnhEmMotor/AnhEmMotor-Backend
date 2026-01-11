
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed record UpdateManyProductStatusesCommand(List<int> Ids, string StatusId) : IRequest<Result<List<int>?>>;
