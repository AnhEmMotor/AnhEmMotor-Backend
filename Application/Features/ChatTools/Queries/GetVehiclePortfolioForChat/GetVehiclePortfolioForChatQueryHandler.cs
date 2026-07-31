using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.GetVehiclePortfolioForChat;

public class GetVehiclePortfolioForChatQueryHandler(
    IVehicleReadRepository repo,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetVehiclePortfolioForChatQuery, Result<ChatToolEnvelope<ChatVehiclePortfolioItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatVehiclePortfolioItemDto>>> Handle(
        GetVehiclePortfolioForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Sorts = "-PurchaseDate", Page = 1, PageSize = limit };

        var paged = await repo
            .GetPagedAsync<VehicleResponse>(sieveModel, DataFetchMode.ActiveOnly, null, cancellationToken)
            .ConfigureAwait(false);

        var items = paged.Items ?? [];
        var dtos = items.Select(x => new ChatVehiclePortfolioItemDto
        {
            VehicleId = x.Id,
            FullName = x.FullName,
            PhoneNumber = x.PhoneNumber,
            LicensePlate = x.LicensePlate,
            VinNumber = x.VinNumber,
            BrandName = x.BrandName,
            VariantName = x.VariantName,
            ColorName = x.ColorName,
            PurchaseDate = x.PurchaseDate
        }).ToList();

        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatVehiclePortfolioItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IVehicleReadRepository.GetPagedAsync",
            new Dictionary<string, string>(),
            "danh-muc-xe",
            null);

        return ChatToolEnvelope<ChatVehiclePortfolioItemDto>.Wrap(inner, meta);
    }
}
