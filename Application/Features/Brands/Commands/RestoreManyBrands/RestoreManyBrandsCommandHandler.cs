using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Helpers;
using Mapster;
using MediatR;
using Domain.Enums;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed class RestoreManyBrandsCommandHandler(IBrandReadRepository readRepository, IBrandUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreManyBrandsCommand, (List<BrandResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<BrandResponse>? Data, ErrorResponse? Error)> Handle(RestoreManyBrandsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All).ConfigureAwait(false);
        var deletedBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);

        var allBrandMap = allBrands.ToDictionary(b => b.Id);
        var deletedBrandSet = deletedBrands.Select(b => b.Id).ToHashSet();

        foreach (var id in uniqueIds)
        {
            if (!allBrandMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Brand with Id {id} not found."
                });
            }
            else if (!deletedBrandSet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Brand with Id {id} is not deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if (deletedBrands.ToList().Count > 0)
        {
            updateRepository.Restore(deletedBrands);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (deletedBrands.Adapt<List<BrandResponse>>(), null);
    }
}
