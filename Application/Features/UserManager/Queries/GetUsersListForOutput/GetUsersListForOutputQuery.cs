using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersListForOutput;

public record GetUsersListForOutputQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<UserDTOForOutputResponse>>>;
