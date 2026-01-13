using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Input;

using Mapster;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed class GetInputByIdQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputByIdQuery, Result<InputResponse?>>
{
    public async Task<Result<InputResponse?>> Handle(
        GetInputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var input = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(input is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }

        return input.Adapt<InputResponse>();
    }
}
