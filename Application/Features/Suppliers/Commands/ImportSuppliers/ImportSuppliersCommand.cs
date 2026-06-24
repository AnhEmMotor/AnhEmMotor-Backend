using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Suppliers.Commands.ImportSuppliers;

public sealed class ImportSuppliersResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorFileUrl { get; set; }
    public string? ErrorFileWithReasonUrl { get; set; }
}

public sealed record ImportSuppliersCommand : IRequest<Result<ImportSuppliersResult>>
{
    public IFormFile File { get; init; } = null!;
}
