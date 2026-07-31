using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.SupplierContract;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListSupplierContractsForChat;

public class ListSupplierContractsForChatQueryHandler(
    ISupplierContractReadRepository supplierContractReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListSupplierContractsForChatQuery, Result<ChatToolEnvelope<ChatSupplierContractListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierContractListItemDto>>> Handle(
        ListSupplierContractsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var statusId = string.IsNullOrWhiteSpace(request.StatusId) ? null : request.StatusId.Trim();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel
        {
            Sorts = "-CreatedAt",
            Page = 1,
            PageSize = limit,
            Filters = statusId is null ? null : $"Status=={statusId}"
        };

        var paged = await supplierContractReadRepository
            .GetPagedAsync<SupplierContractResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                c => new ChatSupplierContractListItemDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    SupplierName = c.SupplierName,
                    Status = c.Status,
                    ContractValue = c.ContractValue,
                    EffectiveDate = c.EffectiveDate,
                    ExpirationDate = c.ExpirationDate
                })
            .ToList();

        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatSupplierContractListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (statusId is not null)
        {
            filtersApplied["Trạng thái"] = statusId;
        }

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierContractReadRepository.GetPagedAsync",
            filtersApplied,
            "hop-dong-nha-cung-cap",
            "VND");

        return ChatToolEnvelope<ChatSupplierContractListItemDto>.Wrap(inner, meta);
    }
}
