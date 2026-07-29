using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UpdateSalesContractStatus;

public class UpdateSalesContractStatusCommandHandler(ISalesContractReadRepository readRepo, IUnitOfWork unitOfWork) : IRequestHandler<UpdateSalesContractStatusCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        UpdateSalesContractStatusCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await readRepo.GetByIdAsync(request.ContractId, cancellationToken).ConfigureAwait(false);
        if (contract == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy hợp đồng.");
        if (!SalesContractStatus.IsValid(request.Status))
            return Result<SalesContractResponse>.Failure("Trạng thái hợp đồng không hợp lệ.");

        var normalizedStatus = SalesContractStatus.All
            .First(status => string.Equals(status, request.Status, StringComparison.OrdinalIgnoreCase));
        var isSameStatus = string.Equals(contract.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase);
        var isAllowedTransition =
            isSameStatus ||
            (string.Equals(contract.Status, SalesContractStatus.Draft, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(normalizedStatus, SalesContractStatus.Approved, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(normalizedStatus, SalesContractStatus.PendingApproval, StringComparison.OrdinalIgnoreCase))) ||
            (request.IsAdminApproval &&
             string.Equals(contract.Status, SalesContractStatus.PendingApproval, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(normalizedStatus, SalesContractStatus.Approved, StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(contract.Status, SalesContractStatus.Signed, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(normalizedStatus, SalesContractStatus.Fulfilled, StringComparison.OrdinalIgnoreCase));

        if (!isAllowedTransition)
            return Result<SalesContractResponse>.Failure(
                $"Không thể chuyển hợp đồng từ {contract.Status} sang {normalizedStatus}.");
        contract.Status = normalizedStatus;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<SalesContractResponse>.Success(contract.Adapt<SalesContractResponse>());
    }
}
