using Application.ApiContracts.Brand;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandUpdateService
    {
        Task<bool> UpdateBrandAsync(int id, UpdateBrandRequest request);
        Task<bool> RestoreBrandAsync(int id);
        Task<bool> RestoreBrandsAsync(RestoreManyBrandsRequest request);
    }
}
