using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBrandCommand, (ApiContracts.Brand.Responses.BrandResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Brand.Responses.BrandResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." } ]
            });
        }

        request.Adapt(brand);

        updateRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (brand.Adapt<ApiContracts.Brand.Responses.BrandResponse>(), null);
    }
}
