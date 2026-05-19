using Application.Common.Models;
using MediatR;
using Sieve.Models;

namespace Application.Features.Brands.Queries.ExportBrands;

public sealed record ExportBrandsQuery : IRequest<Result<FileStreamResult>>
{
    public SieveModel? SieveModel { get; init; }
}
