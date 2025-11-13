using Application.ApiContracts.Brand;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandInsertService
    {
        Task<BrandResponse> CreateBrandAsync(CreateBrandRequest request);
    }
}
