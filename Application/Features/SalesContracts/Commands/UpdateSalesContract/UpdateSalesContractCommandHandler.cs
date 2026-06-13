using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UpdateSalesContract;

public class UpdateSalesContractCommandHandler(
    ISalesContractReadRepository readRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSalesContractCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        UpdateSalesContractCommand request,
        CancellationToken cancellationToken)
    {
        var contract = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (contract == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy hợp đồng.");

        if (request.SpecialTerms != null) contract.SpecialTerms = request.SpecialTerms;
        if (request.WarrantyPeriod != null) contract.WarrantyPeriod = request.WarrantyPeriod;
        if (request.WarrantyScope != null) contract.WarrantyScope = request.WarrantyScope;
        if (request.Note != null) contract.Note = request.Note;

        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<SalesContractResponse>.Success(contract.Adapt<SalesContractResponse>());
    }
}
