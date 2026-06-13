using Application.ApiContracts.InventoryReport.Responses;
using System.Linq;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandReadRepository
{
    public Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken);

    public Task<List<InventoryReportSummaryRowResponse>> GetInventoryReportSummaryRowsAsync(
        string? searchTerm,
        CancellationToken cancellationToken);

    public Task<InventoryReportSummaryPageResponse> GetInventoryReportSummaryAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken);
}

