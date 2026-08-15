using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistoryForChat;

public class GetProductViewHistoryForChatQueryHandler(IProductViewRepository productViewRepository) : IRequestHandler<GetProductViewHistoryForChatQuery, Result<List<ProductViewHistoryDto>>>
{
    public async Task<Result<List<ProductViewHistoryDto>>> Handle(GetProductViewHistoryForChatQuery request, CancellationToken cancellationToken)
    {
        var history = await productViewRepository.GetProductViewHistoryForChatAsync(
            request.CustomerId,
            request.VisitorKey,
            request.Limit,
            cancellationToken);

        return Result<List<ProductViewHistoryDto>>.Success(history);
    }
}
