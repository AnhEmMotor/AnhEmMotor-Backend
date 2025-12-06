using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public record ChangeMultipleUsersStatusCommand(ChangeMultipleUsersStatusRequest Model) : IRequest<ChangeStatusMultiUserByManagerResponse>;
