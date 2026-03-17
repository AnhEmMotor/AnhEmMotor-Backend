using Application.ApiContracts.Option.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Options.Queries.GetOptionsList;

public sealed record GetOptionsListQuery : IRequest<Result<List<OptionResponse>>>;
