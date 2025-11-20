using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed class RestoreBrandCommandHandler(IBrandSelectRepository selectRepository, IBrandUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreBrandCommand, (BrandResponse? Data, ErrorResponse? Error)>
{
    public async Task<(BrandResponse? Data, ErrorResponse? Error)> Handle(RestoreBrandCommand request, CancellationToken cancellationToken)
    {
        var brandList = await selectRepository.GetDeletedBrandsByIdsAsync([request.Id], cancellationToken).ConfigureAwait(false);

        if (brandList.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found in deleted brands." }]
            });
        }

        var brand = brandList[0];
        updateRepository.RestoreBrand(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description
        }, null);
    }
}
