using Application.Common.Models;
using MediatR;

namespace Application.Features.ConversionTools.Queries.GetConversionTools;

public record GetConversionToolsQuery : IRequest<Result<List<Application.ApiContracts.ConversionTools.Responses.ConversionToolResponse>>>;
