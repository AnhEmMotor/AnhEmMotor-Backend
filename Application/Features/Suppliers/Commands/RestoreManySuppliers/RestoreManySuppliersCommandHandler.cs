using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandHandler(ISupplierSelectRepository selectRepository, ISupplierUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreManySuppliersCommand, (List<SupplierResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<SupplierResponse>? Data, ErrorResponse? Error)> Handle(RestoreManySuppliersCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        var allSuppliers = await selectRepository.GetAllSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var deletedSuppliers = await selectRepository.GetDeletedSuppliersByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allSupplierMap = allSuppliers.ToDictionary(s => s.Id);
        var deletedSupplierSet = deletedSuppliers.Select(s => s.Id).ToHashSet();

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
            else if (!deletedSupplierSet.Contains(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Supplier with Id {id} is not deleted."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if (deletedSuppliers.Count > 0)
        {
            updateRepository.Restore(deletedSuppliers);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedSuppliers.Select(s => new SupplierResponse
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            Email = s.Email,
            StatusId = s.StatusId
        }).ToList();

        return (responses, null);
    }
}
