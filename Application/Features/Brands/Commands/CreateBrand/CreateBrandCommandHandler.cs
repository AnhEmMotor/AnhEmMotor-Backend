using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler(
    IBrandInsertRepository repository,
    IBrandReadRepository brandReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBrandCommand, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var cleanName = request.Name?.Trim();
        if (cleanName == null)
            return Error.BadRequest("Name is empty/null, please check again");
        var existingBrands = await brandReadRepository.GetByNameAsync(cleanName, cancellationToken)
            .ConfigureAwait(false);
        if (existingBrands.Count != 0)
        {
            return Result<BrandResponse>.Failure("Brand name already exists.");
        }
        var brand = request.Adapt<BrandEntity>();
        brand.Name = request.Name?.Trim();
        brand.Description = request.Description?.Trim();
        brand.Origin = request.Origin?.Trim();
        brand.LogoUrl = request.LogoUrl?.Trim();
        repository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return brand.Adapt<BrandResponse>();
    }
}
