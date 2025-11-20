using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandCommandHandler(IBrandSelectRepository selectRepository, IBrandUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBrandCommand, (BrandResponse? Data, ErrorResponse? Error)>
{
    public async Task<(BrandResponse? Data, ErrorResponse? Error)> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await selectRepository.GetBrandByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (brand == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." }]
            });
        }

        if (request.Name is not null)
            brand.Name = request.Name;

        if (request.Description is not null)
            brand.Description = request.Description;

        updateRepository.UpdateBrand(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description
        }, null);
    }
}
