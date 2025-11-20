using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed class UpdateSupplierStatusCommandHandler(ISupplierSelectRepository selectRepository, ISupplierUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSupplierStatusCommand, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(UpdateSupplierStatusCommand request, CancellationToken cancellationToken)
    {
        var supplier = await selectRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {request.Id} not found." }]
            });
        }

        if (request.Status is not null)
        {
            supplier.StatusId = request.Status;
        }

        updateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            StatusId = supplier.StatusId
        }, null);
    }
}
