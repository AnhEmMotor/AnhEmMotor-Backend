using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSupplierCommand, (SupplierResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(supplier == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"Supplier with Id {request.Id} not found." } ]
            });
        }

        request.Adapt(supplier);

        updateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (supplier.Adapt<SupplierResponse>(), null);
    }
}