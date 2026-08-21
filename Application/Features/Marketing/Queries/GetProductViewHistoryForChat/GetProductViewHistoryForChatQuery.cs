using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public record GetProductViewHistoryForChatQuery(string? VisitorKey, string? CustomerKeyword, int Limit = ChatToolLimit.Default)
    : IRequest<Result<ChatToolEnvelope<ProductViewHistoryDto>>>;
