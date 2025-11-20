using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandHandler(ISupplierSelectRepository selectRepository, ISupplierDeleteRepository deleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManySuppliersCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManySuppliersCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allSuppliers = await selectRepository.GetAllSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var activeSuppliers = await selectRepository.GetActiveSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var activeSupplierSet = activeSuppliers.Select(s => s.Id).ToHashSet();

        foreach (var id in uniqueIds)
        {
            if (!allSupplierMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Supplier with Id {id} not found."
                });
            }
            else if (!activeSupplierSet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Supplier with Id {id} has already been deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeSuppliers.Count > 0)
        {
            deleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
