using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.DeleteUserByManager;

public sealed record DeleteUserByManagerCommand(Guid UserId, Guid CurrentUserId) : IRequest<Result>;
