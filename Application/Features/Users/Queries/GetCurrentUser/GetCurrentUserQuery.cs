using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery(string? UserId) : IRequest<Result<UserResponse>>;
