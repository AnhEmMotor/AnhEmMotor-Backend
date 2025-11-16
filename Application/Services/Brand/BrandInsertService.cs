using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Services.Brand
{
    public class BrandInsertService(IBrandInsertRepository brandInsertRepository) : IBrandInsertService
    {
        public async Task<BrandResponse> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken)
        {
            var brand = new BrandEntity
            {
                Name = request.Name,
                Description = request.Description
            };

            var createdBrand = await brandInsertRepository.AddBrandAsync(brand, cancellationToken).ConfigureAwait(false);

            return new BrandResponse
            {
                Id = createdBrand.Id,
                Name = createdBrand.Name,
                Description = createdBrand.Description
            };
        }
    }
}
