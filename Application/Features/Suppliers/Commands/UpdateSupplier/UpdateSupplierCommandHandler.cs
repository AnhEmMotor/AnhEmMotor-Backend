using Mapster;
using MediatR;
using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSupplierCommand, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {request.Id} not found." }]
            });
        }

        request.Adapt(supplier);

        updateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (supplier.Adapt<SupplierResponse>(), null);
    }
}