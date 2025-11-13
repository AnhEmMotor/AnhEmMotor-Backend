using Application.ApiContracts.Brand;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandDeleteService
    {
        Task<bool> DeleteBrandAsync(int id);
        Task<bool> DeleteBrandsAsync(DeleteManyBrandsRequest request);
    }
}
