using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.DeleteFinanceContract;

public sealed class DeleteFinanceContractCommandHandler(
    IFinanceContractReadRepository readRepo,
    IFinanceContractDeleteRepository deleteRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteFinanceContractCommand, Result>
{
    public async Task<Result> Handle(DeleteFinanceContractCommand request, CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return Result.Failure("Finance contract not found.");
        deleteRepo.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
