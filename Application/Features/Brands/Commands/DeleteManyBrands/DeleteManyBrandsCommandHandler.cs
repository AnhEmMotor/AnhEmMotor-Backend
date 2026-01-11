using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed class DeleteManyBrandsCommandHandler(
    IBrandReadRepository readRepository,
    IBrandDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyBrandsCommand, Result>
{
    public async Task<Result> Handle(
        DeleteManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids == null || request.Ids.Count == 0)
        {
            return Result.Failure(Error.BadRequest("You not pass ID to requests"));
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();

        var allBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allBrandMap = allBrands.ToDictionary(b => b.Id);
        var activeBrandSet = activeBrands.Select(b => b.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allBrandMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Brand with Id {id} not found.", "Id"));
            } 
            else if(!activeBrandSet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Brand with Id {id} has already been deleted.", "Id"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(activeBrands.ToList().Count > 0)
        {
            deleteRepository.Delete(activeBrands);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
