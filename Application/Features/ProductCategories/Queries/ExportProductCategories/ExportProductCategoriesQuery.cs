using Application.Common.Models;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.ExportProductCategories;

public sealed record ExportProductCategoriesQuery : IRequest<Result<FileStreamResult>>
{
    public SieveModel? SieveModel { get; set; }
}
