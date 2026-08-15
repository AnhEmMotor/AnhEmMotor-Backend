using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public record GetProductViewHistoryForChatQuery(string? VisitorKey, Guid? CustomerId, int Limit = 10) : IRequest<Result<List<ProductViewHistoryDto>>>;
