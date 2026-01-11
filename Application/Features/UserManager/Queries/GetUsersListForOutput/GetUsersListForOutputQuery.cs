using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.UserManager.Queries.GetUsersListForOutput;

public sealed record GetUsersListForOutputQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<UserDTOForOutputResponse>>>;
