using Application.ApiContracts.Option.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Option;
using Mapster;
using MediatR;

namespace Application.Features.Options.Queries.GetOptionsList;

public sealed class GetOptionsListQueryHandler(IOptionReadRepository readRepository) : IRequestHandler<GetOptionsListQuery, Result<List<OptionResponse>>>
{
    public async Task<Result<List<OptionResponse>>> Handle(
        GetOptionsListQuery request,
        CancellationToken cancellationToken)
    {
        var options = await readRepository.GetAllWithOptionsAsync(cancellationToken).ConfigureAwait(false);

        var response = options.Adapt<List<OptionResponse>>();

        return response;
    }
}
