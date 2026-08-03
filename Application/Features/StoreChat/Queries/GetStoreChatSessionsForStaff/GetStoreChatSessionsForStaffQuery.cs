using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetStoreChatSessionsForStaff;

public record GetStoreChatSessionsForStaffQuery : IRequest<Result<List<StoreChatSessionListItemDto>>>;
