using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed class DeleteSupplierCommandHandler(ISupplierSelectRepository selectRepository, ISupplierDeleteRepository deleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSupplierCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await selectRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {request.Id} not found." }]
            };
        }

        deleteRepository.Delete(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
