using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.CloneManyBrands;

public class CloneManyBrandsCommandHandler(
    IBrandReadRepository brandReadRepository,
    IBrandInsertRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneManyBrandsCommand, Result<List<BrandResponse>>>
{
    public async Task<Result<List<BrandResponse>>> Handle(CloneManyBrandsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return Result<List<BrandResponse>>.Failure(Error.BadRequest("No brands selected to clone."));
        }

        var clonedBrands = new List<BrandEntity>();

        foreach (var id in request.Ids)
        {
            var existingBrand = await brandReadRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existingBrand != null)
            {
                var newBrand = new BrandEntity
                {
                    Name = existingBrand.Name + " - Copy",
                    Origin = existingBrand.Origin,
                    LogoUrl = existingBrand.LogoUrl,
                    Description = existingBrand.Description
                };
                
                clonedBrands.Add(newBrand);
                repository.Add(newBrand);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return clonedBrands.Adapt<List<BrandResponse>>();
    }
}
