using Application.ApiContracts.Output.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Output;
using Domain.Entities;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Features.ChatTools.Queries.ListOrdersForChat;

public class ListOrdersForChatQueryHandler(
    IOutputReadRepository outputReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListOrdersForChatQuery, Result<ChatToolEnvelope<ChatOrderListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatOrderListItemDto>>> Handle(
        ListOrdersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var statusId = string.IsNullOrWhiteSpace(request.StatusId) ? null : request.StatusId.Trim();

        Expression<Func<Output, bool>> filter = statusId is null
            ? output => output.CreatedAt != null && output.CreatedAt >= start && output.CreatedAt <= end
            : output => output.CreatedAt != null && output.CreatedAt >= start && output.CreatedAt <= end &&
                output.StatusId == statusId;

        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel
        {
            Sorts = "-CreatedAt",
            Page = 1,
            PageSize = limit
        };

        var result = await outputReadRepository
            .GetPagedAsync<OutputItemResponse>(sieveModel, filter: filter, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var items = result.Items ?? [];
        var dtos = items
            .Select(
                o => new ChatOrderListItemDto
                {
                    OrderId = o.Id,
                    CustomerName = o.CustomerName ?? o.BuyerName,
                    StatusId = o.StatusId,
                    Total = o.Total,
                    CreatedAt = o.CreatedAt
                })
            .ToList();

        var totalCount = (int)(result.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatOrderListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var filtersApplied = new Dictionary<string, string>
        {
            ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end)
        };
        if (statusId is not null)
        {
            filtersApplied["Trạng thái"] = statusId;
        }

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IOutputReadRepository.GetPagedAsync",
            filtersApplied,
            "don-hang",
            "VND");

        return ChatToolEnvelope<ChatOrderListItemDto>.Wrap(inner, meta);
    }
}
