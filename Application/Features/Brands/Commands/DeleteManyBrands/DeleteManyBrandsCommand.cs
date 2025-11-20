using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed record DeleteManyBrandsCommand(List<int> Ids) : IRequest<ErrorResponse?>;
