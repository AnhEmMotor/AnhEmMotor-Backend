using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Application.Interfaces.Repositories.Supplier;

namespace Application.Features.DebtPayments.Queries.GetSuppliersWithDebt
{
    public sealed class GetSuppliersWithDebtQueryHandler(ISupplierDebtRepository supplierDebtRepository)
        : IRequestHandler<GetSuppliersWithDebtQuery, Result<List<SupplierDebtResponse>>>
    {
        public async Task<Result<List<SupplierDebtResponse>>> Handle(
            GetSuppliersWithDebtQuery request,
            CancellationToken cancellationToken)
        {
            var debts = await supplierDebtRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            var supplierDebts = new Dictionary<int, (string Name, string? Phone, decimal TotalDebt)>();

            foreach (var debt in debts)
            {
                var supplier = debt.Supplier;
                if (supplier == null)
                {
                    continue;
                }

                decimal remainingDebt = debt.TotalAmount - debt.PaidAmount;

                if (supplierDebts.TryGetValue(supplier.Id, out var existing))
                {
                    supplierDebts[supplier.Id] = (existing.Name, existing.Phone, existing.TotalDebt + remainingDebt);
                }
                else
                {
                    supplierDebts[supplier.Id] = (supplier.Name ?? string.Empty, supplier.Phone, remainingDebt);
                }
            }

            var result = supplierDebts
                .Where(kvp => kvp.Value.TotalDebt > 0)
                .Select(kvp => new SupplierDebtResponse
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                    Phone = kvp.Value.Phone,
                    TotalDebt = kvp.Value.TotalDebt
                })
                .ToList();

            return result;
        }
    }
}
