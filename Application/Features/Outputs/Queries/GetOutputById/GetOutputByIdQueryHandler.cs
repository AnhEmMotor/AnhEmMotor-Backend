using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;

using Mapster;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed class GetOutputByIdQueryHandler(IOutputReadRepository repository) : IRequestHandler<GetOutputByIdQuery, Result<OutputResponse?>>
{
    public async Task<Result<OutputResponse?>> Handle(
        GetOutputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(output is null)
        {
            return Error.NotFound($"Không tìm thấy đơn hàng có ID {request.Id}.");
        }

        return output.Adapt<OutputResponse>();
    }
}
