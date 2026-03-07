using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;

using Mapster;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed class GetOutputByIdQueryHandler(IOutputReadRepository repository) : IRequestHandler<GetOutputByIdQuery, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(GetOutputByIdQuery request, CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (output is null)
        {
            return Error.NotFound($"Output with Id {request.Id} not found.", nameof(request.Id));
        }

        return output.Adapt<OrderDetailResponse>();
    }
}
