using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed class RestoreBrandCommandHandler(
    IBrandReadRepository readRepository,
    IBrandUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreBrandCommand, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(RestoreBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(brand == null)
        {
            return Error.NotFound($"Brand with Id {request.Id} not found in deleted brands.", "Id");
        }

        updateRepository.Restore(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return brand.Adapt<BrandResponse>();
    }
}
