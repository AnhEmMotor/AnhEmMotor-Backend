using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler(IBrandInsertRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = new BrandEntity
        {
            Name = request.Name,
            Description = request.Description
        };

        repository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description
        };
    }
}
