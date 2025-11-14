using Application.ApiContracts.Brand;
using Domain.Helpers;

namespace Application.Interfaces.Services.Brand
{
    public interface IBrandDeleteService
    {
        Task<ErrorResponse?> DeleteBrandAsync(int id);
        Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request);
    }
}
