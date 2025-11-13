using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandDeleteService
    {
        Task<bool> DeleteBrandAsync(int id);
        Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request);
    }
}
