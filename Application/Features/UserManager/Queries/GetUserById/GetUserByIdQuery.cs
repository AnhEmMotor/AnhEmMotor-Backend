using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Queries.GetUserById;

public record GetUserByIdQuery : IRequest<Result<UserDTOForManagerResponse>>
{
    public Guid? UserId { get; init; }
}
