using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Queries.GetGenderOptions;

public record GetGenderOptionsQuery : IRequest<Result<IEnumerable<GenderOptionResponse>>>;
