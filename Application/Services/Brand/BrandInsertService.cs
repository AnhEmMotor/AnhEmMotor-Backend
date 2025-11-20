using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Services.Brand
{
    public class BrandInsertService(IBrandInsertRepository brandInsertRepository, IUnitOfWork unitOfWork) : IBrandInsertService
    {
        public async Task<BrandResponse> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken)
        {
            var brand = new BrandEntity
            {
                Name = request.Name,
                Description = request.Description
            };

            await brandInsertRepository.AddAsync(brand, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            };
        }
    }
}
