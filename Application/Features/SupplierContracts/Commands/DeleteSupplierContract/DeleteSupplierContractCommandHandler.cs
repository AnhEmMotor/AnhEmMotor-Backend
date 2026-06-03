using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.DeleteSupplierContract;

public sealed class DeleteSupplierContractCommandHandler(
    ISupplierContractReadRepository readRepo,
    ISupplierContractDeleteRepository deleteRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteSupplierContractCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplierContractCommand request, CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
            return Result.Failure("Supplier contract not found.");

        deleteRepo.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
