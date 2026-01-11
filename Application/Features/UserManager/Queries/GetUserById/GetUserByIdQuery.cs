using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDTOForManagerResponse>>;
