using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler(IBrandSelectRepository repository)
    : IRequestHandler<GetBrandByIdQuery, (BrandResponse? Data, ErrorResponse? Error)>
{
    public async Task<(BrandResponse? Data, ErrorResponse? Error)> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await repository.GetBrandByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (brand == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {request.Id} not found." }]
            });
        }

        return (new BrandResponse
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description
        }, null);
    }
}
