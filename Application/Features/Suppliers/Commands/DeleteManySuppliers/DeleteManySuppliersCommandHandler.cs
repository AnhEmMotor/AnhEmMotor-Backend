using MediatR;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using Domain.Enums;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManySuppliersCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManySuppliersCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All).ConfigureAwait(false);
        var activeSuppliers = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

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

        if (activeSuppliers.ToList().Count > 0)
        {
            deleteRepository.Delete(activeSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}