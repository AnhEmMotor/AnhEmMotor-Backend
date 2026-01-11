using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Mapster;
using Application.ApiContracts.Brand.Responses;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreManyBrands;

public sealed class RestoreManyBrandsCommandHandler(
    IBrandReadRepository readRepository,
    IBrandUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyBrandsCommand, Result<List<BrandResponse>?>>
{
    public async Task<Result<List<BrandResponse>?>> Handle(
        RestoreManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();

        var allBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var deletedBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var allBrandMap = allBrands.ToDictionary(b => b.Id);
        var deletedBrandSet = deletedBrands.Select(b => b.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allBrandMap.ContainsKey(id))
            {
                return Error.NotFound($"Brand with Id {id} not found.", "Id");
            } 
            else if(!deletedBrandSet.Contains(id))
            {
                return Error.NotFound($"Brand with Id {id} is not deleted.", "Id");
            }
        }

        if(deletedBrands.ToList().Count > 0)
        {
            updateRepository.Restore(deletedBrands);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return deletedBrands.Adapt<List<BrandResponse>>();
    }
}
