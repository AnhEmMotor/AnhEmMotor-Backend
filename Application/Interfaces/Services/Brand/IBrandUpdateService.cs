using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandUpdateService
    {
        Task<ErrorResponse?> RestoreBrandAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> RestoreBrandsAsync(RestoreManyBrandsRequest request, CancellationToken cancellationToken);
        Task<ErrorResponse?> UpdateBrandAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken);
    }
}
