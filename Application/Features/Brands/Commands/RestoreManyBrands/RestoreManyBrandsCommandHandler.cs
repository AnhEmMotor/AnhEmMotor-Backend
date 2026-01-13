using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Entities;
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

        var existingBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var existingBrandsMap = existingBrands.ToDictionary(b => b.Id);
        var brandsToRestore = new List<Brand>();
        var errors = new List<Error>();

        foreach (var id in uniqueIds)
        {
            if (!existingBrandsMap.TryGetValue(id, out var brand))
            {
                errors.Add(Error.NotFound($"Brand with Id {id} not found.", "Id"));
                continue;
            }

            if (brand.DeletedAt == null)
            {
                errors.Add(Error.BadRequest($"Brand with Id {id} is not deleted.", "Id"));
                continue;
            }

            brandsToRestore.Add(brand);
        }

        if (errors.Count > 0)
        {
            if (typeof(Result<List<BrandResponse>?>).GetMethod("Failure", [typeof(List<Error>)]) != null)
            {
                 return (Result<List<BrandResponse>?>)(object)Result.Failure(errors); 
            }
            return (Result<List<BrandResponse>?>)(object)Result.Failure(errors[0]); 
        }

        if (brandsToRestore.Count > 0)
        {
            updateRepository.Restore(brandsToRestore);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return brandsToRestore.Adapt<List<BrandResponse>>();
    }
}