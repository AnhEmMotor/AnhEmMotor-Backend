using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Queries.GetAssignableUsers;

public record GetAssignableUsersQuery : IRequest<Result<List<UserDTOForOutputResponse>>>;
