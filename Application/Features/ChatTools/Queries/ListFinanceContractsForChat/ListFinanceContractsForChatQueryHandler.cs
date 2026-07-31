using Application.ApiContracts.FinanceContract.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.FinanceContract;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListFinanceContractsForChat;

public class ListFinanceContractsForChatQueryHandler(
    IFinanceContractReadRepository financeContractReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListFinanceContractsForChatQuery, Result<ChatToolEnvelope<ChatFinanceContractListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatFinanceContractListItemDto>>> Handle(
        ListFinanceContractsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var statusId = string.IsNullOrWhiteSpace(request.StatusId) ? null : request.StatusId.Trim();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel
        {
            Sorts = "-CreatedAt",
            Page = 1,
            PageSize = limit,
            Filters = statusId is null ? null : $"DisbursementStatus=={statusId}"
        };

        var paged = await financeContractReadRepository
            .GetPagedAsync<FinanceContractDetailResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                c => new ChatFinanceContractListItemDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    Status = c.Status,
                    CustomerName = c.Customer360?.FullName,
                    PartnerName = c.FinancialPartner?.Name,
                    PrincipalAmount = c.CreditPackage?.PrincipalAmount
                })
            .ToList();

        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatFinanceContractListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (statusId is not null)
        {
            filtersApplied["Trạng thái giải ngân"] = statusId;
        }

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IFinanceContractReadRepository.GetPagedAsync",
            filtersApplied,
            "hop-dong-tai-chinh",
            "VND");

        return ChatToolEnvelope<ChatFinanceContractListItemDto>.Wrap(inner, meta);
    }
}
