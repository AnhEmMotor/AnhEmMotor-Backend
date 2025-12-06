using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand(Guid UserId, ChangeUserStatusRequest Model) : IRequest<ChangeStatusUserByManagerResponse>;
