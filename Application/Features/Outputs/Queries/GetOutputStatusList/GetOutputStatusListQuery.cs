using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputStatusList;

public sealed record GetOutputStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
