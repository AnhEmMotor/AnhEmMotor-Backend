using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UpdateSalesContractStatus;

public class UpdateSalesContractStatusCommandHandler(
    ISalesContractReadRepository readRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSalesContractStatusCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        UpdateSalesContractStatusCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await readRepo.GetByIdAsync(request.ContractId, cancellationToken).ConfigureAwait(false);
        if (contract == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy hợp đồng.");

        contract.Status = request.Status;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        if (string.Compare(request.Status, SalesContractStatus.Signed) == 0 && !contract.SignedDate.HasValue)
            contract.SignedDate = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<SalesContractResponse>.Success(contract.Adapt<SalesContractResponse>());
    }
}
