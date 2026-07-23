using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetProvinces;

public sealed record GetProvincesQuery : IRequest<Result<object>>;
