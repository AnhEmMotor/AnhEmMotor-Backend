using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(ISupplierSelectRepository selectRepository, ISupplierUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSupplierCommand, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await selectRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {request.Id} not found." }]
            });
        }

        if (request.Name is not null)
        {
            supplier.Name = request.Name;
        }

        if (request.Address is not null)
        {
            supplier.Address = request.Address;
        }

        if (request.PhoneNumber is not null)
        {
            supplier.Phone = request.PhoneNumber;
        }

        if (request.Email is not null)
        {
            supplier.Email = request.Email;
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
