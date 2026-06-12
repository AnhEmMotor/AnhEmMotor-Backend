using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.SalesContracts.Commands.CreateSalesContract;

public sealed class CreateSalesContractCommandHandler(
    ISalesContractReadRepository readRepo,
    ISalesContractInsertRepository insertRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSalesContractCommand, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        CreateSalesContractCommand request,
        CancellationToken cancellationToken)
    {
        var contractNumber = $"HDMB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

        var entity = request.Adapt<SalesContract>();
        entity.ContractNumber = contractNumber;
        entity.Status = "Draft";

        insertRepo.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        if (created == null)
            return Result<SalesContractResponse>.Failure("Không thể tạo hợp đồng.");

        return Result<SalesContractResponse>.Success(created.Adapt<SalesContractResponse>());
    }
}
