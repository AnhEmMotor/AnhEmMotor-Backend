using Application.Common.Models;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.ExportSuppliers;

public sealed record ExportSuppliersQuery : IRequest<Result<FileStreamResult>>
{
    public SieveModel? SieveModel { get; init; }
}