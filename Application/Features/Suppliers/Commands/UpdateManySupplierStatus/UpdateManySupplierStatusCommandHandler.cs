using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Common.Models;
using Mapster;
using MediatR;
using SupplierStatusConstants = Domain.Constants.SupplierStatus;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed class UpdateManySupplierStatusCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManySupplierStatusCommand, (List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateManySupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Ids.Count == 0)
        {
            return ([], null);
        }

        if(!SupplierStatusConstants.IsValid(request.StatusId))
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
                    {
                        Field = "StatusId",
                        Message =
                            $"Invalid status '{request.StatusId}'. Must be one of: {string.Join(", ", SupplierStatusConstants.AllowedValues)}"
                    } ]
            });
        }

        var errorDetails = new List<Common.Models.ErrorDetail>();
        var ids = request.Ids.Distinct().ToList();

        var suppliers = await readRepository.GetByIdAsync(ids, cancellationToken).ConfigureAwait(false);
        var supplierMap = suppliers.ToDictionary(s => s.Id);

        foreach(var id in ids)
        {
            if(!supplierMap.ContainsKey(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Field = "Id", Message = $"Supplier with Id {id} not found." });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errorDetails });
        }

        foreach(var supplier in suppliers)
        {
            supplier.StatusId = request.StatusId;
            updateRepository.Update(supplier);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (suppliers.Adapt<List<SupplierResponse>>(), null);
    }
}