using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.PurchaseRequests.Commands.ImportPurchaseRequests;

public sealed class ImportPurchaseRequestsResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorFileUrl { get; set; }
    public string? ErrorFileWithReasonUrl { get; set; }
}

public sealed record ImportPurchaseRequestsCommand : IRequest<Result<ImportPurchaseRequestsResult>>
{
    public IFormFile File { get; init; } = null!;
}
