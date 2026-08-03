using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetProductVariantsForStaff;

public record GetProductVariantsForStaffQuery(int ProductId) : IRequest<Result<List<StoreChatVariantCardDto>>>;
