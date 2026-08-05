using Application.Common.Models;
using MediatR;

namespace Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;

public record LinkStoreChatSessionToCustomerCommand(Guid SessionId) : IRequest<Result<bool>>;
