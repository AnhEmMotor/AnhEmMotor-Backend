using Application.Common.Models;
using MediatR;

namespace Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;

public sealed record GetPredefinedOptionsListQuery : IRequest<Result<Dictionary<string, string>>>;
