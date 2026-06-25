using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.InventoryReceipts.Commands.ImportInventoryReceipts;

public sealed class ImportInventoryReceiptsResult
{
    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public string? ErrorFileUrl { get; set; }

    public string? ErrorFileWithReasonUrl { get; set; }
}

public sealed record ImportInventoryReceiptsCommand : IRequest<Result<ImportInventoryReceiptsResult>>
{
    public IFormFile File { get; init; } = null!;
}
