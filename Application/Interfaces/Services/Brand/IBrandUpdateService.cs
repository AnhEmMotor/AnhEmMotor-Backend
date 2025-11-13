using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandUpdateService
    {
        Task<bool> UpdateBrandAsync(int id, UpdateBrandRequest request);
        Task<bool> RestoreBrandAsync(int id);
        Task<ErrorResponse?> RestoreBrandsAsync(RestoreManyBrandsRequest request);
    }
}
