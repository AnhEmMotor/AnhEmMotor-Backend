using Application.ApiContracts.FinanceContract.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using Domain.Entities;
using Sieve.Models;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.CreateFinanceContract;

public sealed class CreateFinanceContractCommandHandler(
    IFinanceContractReadRepository readRepo,
    IFinanceContractInsertRepository insertRepo,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateFinanceContractCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateFinanceContractCommand request,
        CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Duplicate number check
        var existing = await readRepo
            .GetPagedAsync<FinanceContract>(
                new SieveModel { Filters = $"ContractNumber == \"{req.ContractNumber}\"" },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (existing.TotalCount > 0)
            return Result<Guid>.Failure("Contract number already exists.");

        var entity = new FinanceContract
        {
            Id = Guid.NewGuid(),
            ContractNumber = req.ContractNumber,
            CustomerId = req.CustomerId,
            BankName = req.BankName,
            LoanAmount = req.LoanAmount,
            TermMonths = req.TermMonths,
            InterestRate = req.InterestRate,
            DisbursementStatus = string.IsNullOrWhiteSpace(req.DisbursementStatus)
                ? "Pending" : req.DisbursementStatus,
            CavetLocation = req.CavetLocation ?? "Bank",
            SignedDate = req.SignedDate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = null,
        };

        insertRepo.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(entity.Id);
    }
}
