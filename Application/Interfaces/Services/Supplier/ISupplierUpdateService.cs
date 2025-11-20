using Application.ApiContracts.Supplier;
using Domain.Helpers;

namespace Application.Interfaces.Services.Supplier
{
    public interface ISupplierUpdateService
    {
        Task<(SupplierResponse? Data, ErrorResponse? Error)> UpdateSupplierAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken);
        Task<(SupplierResponse? Data, ErrorResponse? Error)> UpdateSupplierStatusAsync(
            int id,
            UpdateSupplierStatusRequest request,
            CancellationToken cancellationToken
        );
        Task<(SupplierResponse? Data, ErrorResponse? Error)> RestoreSupplierAsync(int id, CancellationToken cancellationToken);
        Task<(List<int>? Data, ErrorResponse? Error)> UpdateManySupplierStatusAsync(UpdateManySupplierStatusRequest request, CancellationToken cancellationToken);
        Task<(List<int>? Data, ErrorResponse? Error)> RestoreSuppliersAsync(RestoreManySuppliersRequest request, CancellationToken cancellationToken);
    }
}
