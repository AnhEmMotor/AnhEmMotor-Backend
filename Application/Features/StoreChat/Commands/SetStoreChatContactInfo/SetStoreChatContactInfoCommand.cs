using Application.Common.Models;
using MediatR;

namespace Application.Features.StoreChat.Commands.SetStoreChatContactInfo;

public record SetStoreChatContactInfoCommand(Guid SessionId, string ContactName, string ContactPhone) : IRequest<Result<bool>>;
