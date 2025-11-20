using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed class RestoreSupplierCommandHandler(ISupplierSelectRepository selectRepository, ISupplierUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreSupplierCommand, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(RestoreSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await selectRepository.GetDeletedSuppliersByIdsAsync([request.Id], cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Deleted supplier with Id {request.Id} not found." }]
            });
        }

        updateRepository.Restore(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new SupplierResponse
        {
            Id = supplier[0].Id,
            Name = supplier[0].Name,
            Address = supplier[0].Address,
            Phone = supplier[0].Phone,
            Email = supplier[0].Email,
            StatusId = supplier[0].StatusId
        }, null);
    }
}
