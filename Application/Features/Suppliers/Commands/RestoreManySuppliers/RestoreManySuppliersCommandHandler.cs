using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManySuppliersCommand, (List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Common.Models.ErrorDetail>();

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
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "Id", Message = $"Supplier with Id {id} not found." });
            } else if(!deletedSupplierSet.Contains(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "Id", Message = $"Supplier with Id {id} is not deleted." });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errorDetails });
        }

        if(deletedSuppliers.ToList().Count > 0)
        {
            updateRepository.Restore(deletedSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (deletedSuppliers.Adapt<List<SupplierResponse>>(), null);
    }
}