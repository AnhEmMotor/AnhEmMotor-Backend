using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed class DeleteSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSupplierCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(supplier == null)
        {
            return Result.Failure(Error.NotFound($"Supplier with Id {request.Id} not found."));
        }

        deleteRepository.Delete(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
