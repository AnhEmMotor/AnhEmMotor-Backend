using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandUpdateService
    {
        Task<ErrorResponse?> RestoreBrandAsync(int id);
        Task<ErrorResponse?> RestoreBrandsAsync(RestoreManyBrandsRequest request);
        Task<ErrorResponse?> UpdateBrandAsync(int id, UpdateBrandRequest request);
    }
}
