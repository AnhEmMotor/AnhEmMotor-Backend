using Domain.Primitives;

namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractListResponse(
    List<SupplierContractResponse>? items,
    long? totalCount,
    int? pageNumber,
    int? pageSize) : PagedResult<SupplierContractResponse>(items, totalCount, pageNumber, pageSize)
{
}
