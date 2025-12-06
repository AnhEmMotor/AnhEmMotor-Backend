using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories.Output;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed class GetOutputByIdQueryHandler(IOutputReadRepository repository) : IRequestHandler<GetOutputByIdQuery, (OutputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(OutputResponse? Data, ErrorResponse? Error)> Handle(
        GetOutputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var output = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(output is null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Không tìm thấy đơn hàng có ID {request.Id}." } ]
            });
        }

        return (output.Adapt<OutputResponse>(), null);
    }
}
