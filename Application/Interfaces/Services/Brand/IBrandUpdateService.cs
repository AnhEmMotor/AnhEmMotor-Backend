using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandUpdateService
    {
        Task<(BrandResponse? Data, ErrorResponse? Error)> UpdateBrandAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken);
        Task<(BrandResponse? Data, ErrorResponse? Error)> RestoreBrandAsync(int id, CancellationToken cancellationToken);
        Task<(List<int>? Data, ErrorResponse? Error)> RestoreBrandsAsync(RestoreManyBrandsRequest request, CancellationToken cancellationToken);
    }
}
