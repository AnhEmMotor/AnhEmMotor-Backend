using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManySuppliersCommand,Result<List<SupplierResponse>?>>
{
    public async Task<Result<List<SupplierResponse>?>> Handle(
        RestoreManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();

        var allSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var deletedSuppliers = await readRepository.GetByIdAsync(
            uniqueIds,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var deletedSupplierSet = deletedSuppliers.Select(s => s.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allSupplierMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Supplier with Id {id} not found."));
            } 
            else if(!deletedSupplierSet.Contains(id))
            {
                errorDetails.Add(Error.BadRequest($"Supplier with Id {id} is not deleted."));
            }
        }

        if(errorDetails.Count > 0)
        {
            return errorDetails;
        }

        if(deletedSuppliers.ToList().Count > 0)
        {
            updateRepository.Restore(deletedSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return deletedSuppliers.Adapt<List<SupplierResponse>>();
    }
}