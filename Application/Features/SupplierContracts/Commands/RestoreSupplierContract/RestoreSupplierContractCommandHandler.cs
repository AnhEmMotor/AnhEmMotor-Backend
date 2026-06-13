using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.RestoreSupplierContract;

public class RestoreSupplierContractCommandHandler(
    ISupplierContractReadRepository readRepo,
    ISupplierContractUpdateRepository updateRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<RestoreSupplierContractCommand, Result>
{
    public async Task<Result> Handle(RestoreSupplierContractCommand request, CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);
        if (entity == null)
            return Result.Failure("Supplier contract not found.");

        updateRepo.Restore(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
