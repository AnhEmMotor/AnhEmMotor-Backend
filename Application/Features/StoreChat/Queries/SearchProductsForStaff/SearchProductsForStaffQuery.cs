using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.SearchProductsForStaff;

public record SearchProductsForStaffQuery(string? Keyword, int Limit = 10) : IRequest<Result<List<StoreChatProductSearchItemDto>>>;
