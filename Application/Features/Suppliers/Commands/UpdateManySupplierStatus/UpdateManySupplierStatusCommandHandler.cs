using Mapster;
using MediatR;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Domain.Helpers;
using SupplierStatusConstants = Domain.Constants.SupplierStatus;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed class UpdateManySupplierStatusCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManySupplierStatusCommand, (List<SupplierResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<SupplierResponse>? Data, ErrorResponse? Error)> Handle(UpdateManySupplierStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return ([], null);
        }

        if (!SupplierStatusConstants.IsValid(request.StatusId))
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail
                 {
                     Field = "StatusId",
                     Message = $"Invalid status '{request.StatusId}'. Must be one of: {string.Join(", ", SupplierStatusConstants.AllowedValues)}"
                 }]
            });
        }

        var errorDetails = new List<ErrorDetail>();
        var ids = request.Ids.Distinct().ToList();

        var suppliers = await readRepository.GetByIdAsync(ids, cancellationToken).ConfigureAwait(false);
        var supplierMap = suppliers.ToDictionary(s => s.Id);

        foreach (var id in ids)
        {
            if (!supplierMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"Supplier with Id {id} not found."
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        foreach (var supplier in suppliers)
        {
            supplier.StatusId = request.StatusId;
            updateRepository.Update(supplier);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (suppliers.Adapt<List<SupplierResponse>>(), null);
    }
}