using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed class UpdateManySupplierStatusCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManySupplierStatusCommand, Result<List<SupplierResponse>?>>
{
    public async Task<Result<List<SupplierResponse>?>> Handle(
        UpdateManySupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var errorDetails = new List<Error>();
        var ids = request.Ids.Distinct().ToList();

        var suppliers = await readRepository.GetByIdAsync(ids, cancellationToken).ConfigureAwait(false);
        var supplierMap = suppliers.ToDictionary(s => s.Id);

        foreach(var id in ids)
        {
            if(!supplierMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Supplier with Id {id} not found.", "Id"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return errorDetails;
        }

        foreach(var supplier in suppliers)
        {
            supplier.StatusId = request.StatusId;
            updateRepository.Update(supplier);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return suppliers.Adapt<List<SupplierResponse>>();
    }
}