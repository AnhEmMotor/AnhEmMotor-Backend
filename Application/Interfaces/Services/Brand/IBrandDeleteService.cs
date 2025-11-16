using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandDeleteService
    {
        Task<ErrorResponse?> DeleteBrandAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request, CancellationToken cancellationToken);
    }
}
