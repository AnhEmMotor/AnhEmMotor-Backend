using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManySuppliersCommand, Result>
{
    public async Task<Result> Handle(
        DeleteManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();

        var allSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var activeSupplierSet = activeSuppliers.Select(s => s.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allSupplierMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Supplier with Id {id} not found.", "Id"));
            } 
            else if(!activeSupplierSet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Supplier with Id {id} has already been deleted.", "Id"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(activeSuppliers.ToList().Count > 0)
        {
            deleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}