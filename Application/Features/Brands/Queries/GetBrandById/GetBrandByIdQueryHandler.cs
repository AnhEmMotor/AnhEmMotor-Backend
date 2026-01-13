using Application.Interfaces.Repositories.Brand;
using Application.ApiContracts.Brand.Responses;
using Mapster;
using MediatR;
using Application.Common.Models;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetBrandByIdQuery, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(
        GetBrandByIdQuery request,
        CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return Error.NotFound($"Brand with Id {request.Id} not found.", "Id");
        }

        return brand.Adapt<BrandResponse>();
    }
}
