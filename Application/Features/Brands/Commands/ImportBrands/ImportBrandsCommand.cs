using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

using Microsoft.AspNetCore.Http;

namespace Application.Features.Brands.Commands.ImportBrands;

public sealed record ImportBrandsCommand : IRequest<Result<ImportBrandsResult>>
{
    public IFormFile File { get; init; } = null!;
}
