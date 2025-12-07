using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManySuppliersCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Common.Models.ErrorDetail>();

        var allSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var activeSupplierSet = activeSuppliers.Select(s => s.Id).ToHashSet();

        foreach(var id in uniqueIds)
        {
            if(!allSupplierMap.ContainsKey(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "Id", Message = $"Supplier with Id {id} not found." });
            } else if(!activeSupplierSet.Contains(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Supplier with Id {id} has already been deleted."
                    });
            }
        }

        if(errorDetails.Count > 0)
        {
            return new Common.Models.ErrorResponse { Errors = errorDetails };
        }

        if(activeSuppliers.ToList().Count > 0)
        {
            deleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}