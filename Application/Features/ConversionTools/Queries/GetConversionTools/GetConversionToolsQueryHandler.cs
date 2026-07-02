using Application.ApiContracts.ConversionTools.Responses;
using Application.Common.Models;
using Application.Features.ConversionTools.Queries.GetConversionTools;
using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using MediatR;
using Mapster;

namespace Application.Features.ConversionTools.Queries.GetConversionTools;

public class GetConversionToolsQueryHandler(IConversionToolReadRepository repository)
    : IRequestHandler<GetConversionToolsQuery, Result<List<ConversionToolResponse>>>
{
    public async Task<Result<List<ConversionToolResponse>>> Handle(
        GetConversionToolsQuery request,
        CancellationToken cancellationToken)
    {
        var tools = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var response = tools.Adapt<List<ConversionToolResponse>>();
        return Result<List<ConversionToolResponse>>.Success(response);
    }
}
