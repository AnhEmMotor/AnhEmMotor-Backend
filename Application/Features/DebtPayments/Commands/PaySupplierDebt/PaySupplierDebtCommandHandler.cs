using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierDebt;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.DebtPayments.Commands.PaySupplierDebt
{
    public class PaySupplierDebtCommandHandler(
        ISupplierDebtReadRepository supplierDebtReadRepository,
        ISupplierDebtUpdateRepository supplierDebtUpdateRepository,
        ISupplierDebtInsertRepository supplierDebtInsertRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<PaySupplierDebtCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(PaySupplierDebtCommand request, CancellationToken cancellationToken)
        {
            var debts = await supplierDebtReadRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);
            var unpaidDebts = debts
                .Where(d => d.PaidAmount < d.TotalAmount && d.InventoryReceipt?.StatusId == "approve")
                .OrderBy(d => d.CreatedAt)
                .ToList();
            decimal remainingAmountToPay = request.Amount;
            var currentUserId = currentUserContext.GetUserId();
            foreach (var debt in unpaidDebts)
            {
                if (remainingAmountToPay <= 0)
                {
                    break;
                }
                decimal debtRemaining = debt.TotalAmount - debt.PaidAmount;
                decimal amountToPayForThisDebt = Math.Min(remainingAmountToPay, debtRemaining);
                debt.PaidAmount += amountToPayForThisDebt;
                supplierDebtUpdateRepository.Update(debt);
                remainingAmountToPay -= amountToPayForThisDebt;
            }
            decimal totalSupplierRemainingDebt = debts
                .Where(d => d.InventoryReceipt?.StatusId == "approve")
                .Sum(d => d.TotalAmount - d.PaidAmount);
            var paymentLog = new SupplierDebtLog
            {
                SupplierId = request.SupplierId,
                AmountPaid = request.Amount,
                RemainingDebt = totalSupplierRemainingDebt,
                PaymentDate = DateTimeOffset.UtcNow,
                CreatedById = currentUserId
            };
            await supplierDebtInsertRepository.InsertSupplierDebtLogAsync(paymentLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
