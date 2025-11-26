using Application.ApiContracts.Input;
using Application.Interfaces.Repositories.Input;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed class GetInputByIdQueryHandler(
    IInputReadRepository repository) : IRequestHandler<GetInputByIdQuery, (InputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, ErrorResponse? Error)> Handle(
        GetInputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var input = await repository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken)
            .ConfigureAwait(false);

        if (input is null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Không tìm thấy phiếu nhập có ID {request.Id}." }]
            });
        }

        return (input.Adapt<InputResponse>(), null);
    }
}
