using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using MediatR;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Primitives;

namespace Application.Features.UserManager.Queries.GetUsersListForOutput;

public sealed record GetUsersListForOutputQuery(SieveModel SieveModel) : IRequest<PagedResult<UserDTOForOutputResponse>>;
