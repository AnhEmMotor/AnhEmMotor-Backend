using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using MediatR;

namespace Application.Features.SalesContracts.Commands.DeleteSalesContract;

public sealed class DeleteSalesContractCommandHandler(
    ISalesContractReadRepository readRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSalesContractCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSalesContractCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await readRepo.GetByIdAsync(request.Id, cancellationToken);
        if (contract == null)
            return Result.Failure("Không tìm thấy hợp đồng.");

        contract.DeletedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
