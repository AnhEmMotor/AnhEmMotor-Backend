using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Primitives;
using MediatR;

using System;

using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetSuppliersWithDebt
{
    public class GetSuppliersWithDebtQueryHandler(ISupplierDebtReadRepository supplierDebtRepository) : IRequestHandler<GetSuppliersWithDebtQuery, Result<PagedResult<SupplierDebtResponse>>>
    {
        public async Task<Result<PagedResult<SupplierDebtResponse>>> Handle(
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
                } else
                {
                    supplierDebts[supplier.Id] = (supplier.Name ?? string.Empty, supplier.Phone, remainingDebt);
                }
            }
            var allSuppliers = supplierDebts
                .Where(kvp => kvp.Value.TotalDebt > 0)
                .Select(
                    kvp => new SupplierDebtResponse
                    {
                        Id = kvp.Key,
                        Name = kvp.Value.Name,
                        Phone = kvp.Value.Phone,
                        TotalDebt = kvp.Value.TotalDebt
                    })
                .OrderByDescending(x => x.TotalDebt)
                .ToList();
            var page = request.SieveModel?.Page ?? 1;
            var pageSize = request.SieveModel?.PageSize ?? 10;
            var totalCount = allSuppliers.Count;
            var pagedSuppliers = allSuppliers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var pagedResult = new PagedResult<SupplierDebtResponse>(pagedSuppliers, totalCount, page, pageSize);
            return Result<PagedResult<SupplierDebtResponse>>.Success(pagedResult);
        }
    }
}
