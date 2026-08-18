using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public class GetProductViewHistoryForChatQueryHandler(
    IProductViewRepository productViewRepository,
    IUserReadRepository userReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetProductViewHistoryForChatQuery, Result<ChatToolEnvelope<ProductViewHistoryDto>>>
{
    public async Task<Result<ChatToolEnvelope<ProductViewHistoryDto>>> Handle(
        GetProductViewHistoryForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var keyword = request.CustomerKeyword?.Trim();
        Guid? customerUserId = null;
        var warnings = new List<string>();
        if (!string.IsNullOrEmpty(keyword))
        {
            var user = keyword.Contains('@')
                ? await userReadRepository.FindUserByEmailAsync(keyword, cancellationToken)
                : await userReadRepository.FindUserByPhoneNumberAsync(keyword, cancellationToken);
            if (user == null)
            {
                warnings.Add($"Không tìm thấy tài khoản khách hàng khớp với '{keyword}'.");
            }
            else
            {
                customerUserId = user.Id;
            }
        }
        var history = customerUserId is null && string.IsNullOrEmpty(request.VisitorKey)
            ? []
            : await productViewRepository.GetProductViewHistoryForChatAsync(
                customerUserId,
                request.VisitorKey,
                limit,
                cancellationToken);

        var inner = new ChatToolResult<ProductViewHistoryDto>(history, history.Count, history.Count >= limit);
        var filters = !string.IsNullOrEmpty(keyword)
            ? new Dictionary<string, string> { ["Khách hàng"] = keyword }
            : new Dictionary<string, string> { ["Khách vãng lai"] = request.VisitorKey ?? string.Empty };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IProductViewRepository.GetProductViewHistoryForChatAsync",
            filters,
            "lich-su-xem-san-pham",
            null,
            warnings.Count > 0 ? warnings : null);
        return ChatToolEnvelope<ProductViewHistoryDto>.Wrap(inner, meta);
    }
}
