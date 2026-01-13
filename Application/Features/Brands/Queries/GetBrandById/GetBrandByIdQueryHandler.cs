using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetBrandByIdQuery, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return Error.NotFound($"Brand with Id {request.Id} not found.", "Id");
        }

        return brand.Adapt<BrandResponse>();
    }
}
