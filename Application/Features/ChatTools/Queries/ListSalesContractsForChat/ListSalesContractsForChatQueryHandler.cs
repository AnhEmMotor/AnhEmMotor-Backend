using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.SalesContract;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListSalesContractsForChat;

public class ListSalesContractsForChatQueryHandler(
    ISalesContractReadRepository salesContractReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListSalesContractsForChatQuery, Result<ChatToolEnvelope<ChatSalesContractListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSalesContractListItemDto>>> Handle(
        ListSalesContractsForChatQuery request,
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
        var paged = await salesContractReadRepository
            .GetPagedAsync(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                c => new ChatSalesContractListItemDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    Status = c.Status,
                    CustomerFullName = c.CustomerFullName,
                    CustomerPhone = c.CustomerPhone,
                    VehicleModel = c.VehicleModel,
                    ActualSalePrice = c.ActualSalePrice,
                    SignedDate = c.SignedDate,
                    CreatedAt = c.CreatedAt
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatSalesContractListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (statusId is not null)
        {
            filtersApplied["Trạng thái"] = statusId;
        }
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISalesContractReadRepository.GetPagedAsync",
            filtersApplied,
            "hop-dong-ban-hang",
            "VND");
        return ChatToolEnvelope<ChatSalesContractListItemDto>.Wrap(inner, meta);
    }
}
