using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierStatisticsForChat;

public sealed record GetSupplierStatisticsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierStatisticsDto>>>;
