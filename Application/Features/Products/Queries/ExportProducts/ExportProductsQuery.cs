using Application.Common.Models;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.ExportProducts;

public sealed record ExportProductsQuery : IRequest<Result<FileStreamResult>>
{
    public SieveModel? SieveModel { get; set; }
}