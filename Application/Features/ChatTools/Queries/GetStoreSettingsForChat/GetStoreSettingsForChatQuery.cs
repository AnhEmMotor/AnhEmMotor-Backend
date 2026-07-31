using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetStoreSettingsForChat;

public sealed record GetStoreSettingsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatStoreSettingsDto>>>
{
}
