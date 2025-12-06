using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetBrandByIdQuery, (ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)> Handle(
        GetBrandByIdQuery request,
        CancellationToken cancellationToken)
    {
        var brand = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(brand == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." } ]
            });
        }

        return (brand.Adapt<ApiContracts.Brand.Responses.BrandResponse>(), null);
    }
}
