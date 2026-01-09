using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories.Input;

using Mapster;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed class GetInputByIdQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputByIdQuery, (InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetInputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var input = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(input is null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Message = $"Không tìm thấy phiếu nhập có ID {request.Id}." } ]
            });
        }

        return (input.Adapt<InputResponse>(), null);
    }
}
