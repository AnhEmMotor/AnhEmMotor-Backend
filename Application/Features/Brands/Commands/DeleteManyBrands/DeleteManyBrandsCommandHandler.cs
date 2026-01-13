using Application.Common.Models;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Entities;
using MediatR;

public sealed class DeleteManyBrandsCommandHandler(
    IBrandReadRepository readRepository,
    IBrandDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyBrandsCommand, Result>
{
    public async Task<Result> Handle(DeleteManyBrandsCommand request, CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();

        var existingBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var existingBrandsMap = existingBrands.ToDictionary(b => b.Id);

        var errorDetails = new List<Error>();
        var brandsToDelete = new List<Brand>();

        foreach(var id in uniqueIds)
        {
            if(!existingBrandsMap.TryGetValue(id, out var brand))
            {
                errorDetails.Add(Error.NotFound($"Brand with Id {id} not found.", "Id"));
                continue;
            }

            if(brand.DeletedAt != null)
            {
                errorDetails.Add(Error.BadRequest($"Brand with Id {id} has already been deleted.", "Id"));
                continue;
            }

            brandsToDelete.Add(brand);
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(brandsToDelete.Count > 0)
        {
            foreach(var brand in brandsToDelete)
            {
                deleteRepository.Delete(brand);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}