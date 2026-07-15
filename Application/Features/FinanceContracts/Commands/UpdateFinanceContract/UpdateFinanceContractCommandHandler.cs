using Application.ApiContracts.FinanceContract.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using Domain.Entities;
using global::Sieve.Models;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateFinanceContract;

public sealed class UpdateFinanceContractCommandHandler(
    IFinanceContractReadRepository readRepo,
    IFinanceContractUpdateRepository updateRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateFinanceContractCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpdateFinanceContractCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return Result<Guid>.Failure("Finance contract not found.");

        var req = request.Request;

        // Check duplicate contract number (excluding current entity)
        var allContracts = await readRepo
            .GetPagedAsync<FinanceContract>(
                new SieveModel { Filters = $"ContractNumber == \"{req.ContractNumber}\"" },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (allContracts.TotalCount > 0 && allContracts.Items.Any(x => x.Id != request.Id))
            return Result<Guid>.Failure("Contract number already exists.");

        entity.ContractNumber = req.ContractNumber;
        entity.CustomerId = req.CustomerId;
        entity.BankName = req.BankName;
        entity.LoanAmount = req.LoanAmount;
        entity.TermMonths = req.TermMonths;
        entity.InterestRate = req.InterestRate;
        entity.DisbursementStatus = req.DisbursementStatus ?? entity.DisbursementStatus;
        entity.CavetLocation = req.CavetLocation ?? entity.CavetLocation;
        entity.SignedDate = req.SignedDate ?? entity.SignedDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        updateRepo.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(entity.Id);
    }
}
