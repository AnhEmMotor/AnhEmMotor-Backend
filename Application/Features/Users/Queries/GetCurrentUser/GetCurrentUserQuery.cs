using Application.ApiContracts.User.Responses;
using MediatR;

namespace Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery(string? UserId) : IRequest<UserResponse>;
