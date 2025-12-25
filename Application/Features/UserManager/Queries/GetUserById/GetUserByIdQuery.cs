using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDTOForManagerResponse>;
