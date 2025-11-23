using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed class DeleteManyBrandsCommandHandler(IBrandReadRepository readRepository, IBrandDeleteRepository deleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManyBrandsCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManyBrandsCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All).ConfigureAwait(false);
        var activeBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allBrandMap = allBrands.ToDictionary(b => b.Id);
        var activeBrandSet = activeBrands.Select(b => b.Id).ToHashSet();

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
            else if (!activeBrandSet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Brand with Id {id} has already been deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeBrands.ToList().Count > 0)
        {
            deleteRepository.Delete(activeBrands);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
