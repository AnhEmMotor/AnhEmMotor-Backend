using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Supplier;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId
{
    public class GetInventoryReceiptsBySupplierIdQueryHandler : IRequestHandler<GetInventoryReceiptsBySupplierIdQuery, Result<PagedResult<InventoryReceiptListResponse>>>
    {
        private readonly IInventoryReceiptReadRepository repository;
        private readonly ISupplierReadRepository supplierReadRepository;

        public GetInventoryReceiptsBySupplierIdQueryHandler(
            IInventoryReceiptReadRepository repository,
            ISupplierReadRepository supplierReadRepository)
        {
            this.repository = repository;
            this.supplierReadRepository = supplierReadRepository;
        }

        public async Task<Result<PagedResult<InventoryReceiptListResponse>>> Handle(
            GetInventoryReceiptsBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            var supplier = await supplierReadRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                .ConfigureAwait(false);
            if (supplier is null)
            {
                return Result<PagedResult<InventoryReceiptListResponse>>.Failure(
                    Error.NotFound($"Không tìm thấy nhà cung cấp với Id = {request.SupplierId}"));
            }

            var sieveModel = request.SieveModel ?? new SieveModel();
            var result = await repository.GetPagedAsync<InventoryReceiptListResponse>(
                sieveModel,
                filter: x => x.InventoryReceiptInfos.Any(
                    ii => ii.DeletedAt == null && ii.PurchaseRequestItem != null &&
                          ii.PurchaseRequestItem.SupplierId == request.SupplierId),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
    }
}
