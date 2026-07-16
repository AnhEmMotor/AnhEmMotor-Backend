using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetWards;

public sealed record GetWardsQuery(int ProvinceId) : IRequest<Result<object>>;
