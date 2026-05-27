using Application.Common.Models;
using Application.Interfaces.Repositories.PredefinedOption;
using MediatR;

namespace Application.Features.PredefinedOptions.Queries.GetPredefinedOptionsList;

public sealed class GetPredefinedOptionsListQueryHandler(IPredefinedOptionReadRepository repository) : IRequestHandler<GetPredefinedOptionsListQuery, Result<Dictionary<string, string>>>
{
    public async Task<Result<Dictionary<string, string>>> Handle(
        GetPredefinedOptionsListQuery request,
        CancellationToken cancellationToken)
    {
        var options = await repository
            .GetAllAsDictionaryAsync(cancellationToken)
            .ConfigureAwait(false);
        return Result<Dictionary<string, string>>.Success(options);
    }
}
