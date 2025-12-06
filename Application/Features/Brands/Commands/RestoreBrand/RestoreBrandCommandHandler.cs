using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed class RestoreBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreBrandCommand, (ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)> Handle(
        RestoreBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(brand == null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Brand with Id {request.Id} not found in deleted brands."
                    } ]
            });
        }

        updateRepository.Restore(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (brand.Adapt<ApiContracts.Brand.Responses.BrandResponse>(), null);
    }
}
