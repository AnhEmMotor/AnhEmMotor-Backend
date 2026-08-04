using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarrantyTermsForChat;

public class GetWarrantyTermsForChatQueryHandler(
    IWarrantyTermReadRepository warrantyTermReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetWarrantyTermsForChatQuery, Result<ChatToolEnvelope<ChatWarrantyTermDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatWarrantyTermDto>>> Handle(
        GetWarrantyTermsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var terms = (await warrantyTermReadRepository
            .GetAllAsync(cancellationToken, includeBrand: true, mode: DataFetchMode.ActiveOnly)
            .ConfigureAwait(false)).ToList();
        var ordered = terms.OrderByDescending(t => t.Id).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                t => new ChatWarrantyTermDto
                {
                    TermId = t.Id,
                    BrandName = t.Brand?.Name,
                    TermName = t.TermName,
                    VehicleType = t.VehicleType,
                    ErrorCategory = t.ErrorCategory,
                    DurationMonths = t.DurationMonths,
                    DurationKm = t.DurationKm,
                    Coverage = t.Coverage,
                    Status = t.Status,
                    EffectiveDate = t.EffectiveDate,
                    ExpirationDate = t.ExpirationDate
                })
            .ToList();
        var inner = new ChatToolResult<ChatWarrantyTermDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IWarrantyTermReadRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "dieu-khoan-bao-hanh",
            null);
        return ChatToolEnvelope<ChatWarrantyTermDto>.Wrap(inner, meta);
    }
}
