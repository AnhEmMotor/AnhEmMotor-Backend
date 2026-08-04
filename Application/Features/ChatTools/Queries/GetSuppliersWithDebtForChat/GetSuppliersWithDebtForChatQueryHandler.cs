using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSuppliersWithDebtForChat;

public class GetSuppliersWithDebtForChatQueryHandler(
    ISupplierDebtReadRepository supplierDebtRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetSuppliersWithDebtForChatQuery, Result<ChatToolEnvelope<ChatSupplierDebtListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierDebtListItemDto>>> Handle(
        GetSuppliersWithDebtForChatQuery request,
        CancellationToken cancellationToken)
    {
        var debts = await supplierDebtRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var supplierDebts = new Dictionary<int, (string Name, decimal TotalDebt)>();
        foreach (var debt in debts)
        {
            var supplier = debt.Supplier;
            if (supplier == null)
            {
                continue;
            }
            var remainingDebt = debt.TotalAmount - debt.PaidAmount;
            if (supplierDebts.TryGetValue(supplier.Id, out var existing))
            {
                supplierDebts[supplier.Id] = (existing.Name, existing.TotalDebt + remainingDebt);
            } else
            {
                supplierDebts[supplier.Id] = (supplier.Name ?? string.Empty, remainingDebt);
            }
        }
        var suppliersWithDebt = supplierDebts
            .Where(kvp => kvp.Value.TotalDebt > 0)
            .OrderByDescending(kvp => kvp.Value.TotalDebt)
            .ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = suppliersWithDebt
            .Take(limit)
            .Select(
                kvp => new ChatSupplierDebtListItemDto
                {
                    SupplierId = kvp.Key,
                    SupplierName = kvp.Value.Name,
                    DebtAmount = kvp.Value.TotalDebt
                })
            .ToList();
        var inner = new ChatToolResult<ChatSupplierDebtListItemDto>(
            dtos,
            suppliersWithDebt.Count,
            suppliersWithDebt.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierDebtReadRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "cong-no-nha-cung-cap",
            "VND");
        return ChatToolEnvelope<ChatSupplierDebtListItemDto>.Wrap(inner, meta);
    }
}
