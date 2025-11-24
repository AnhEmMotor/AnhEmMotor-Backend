using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler(IBrandInsertRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = request.Adapt<BrandEntity>();

        repository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return brand.Adapt<BrandResponse>();
    }
}
