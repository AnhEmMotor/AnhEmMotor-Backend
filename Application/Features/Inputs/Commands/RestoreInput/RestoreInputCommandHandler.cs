using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed class RestoreInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInputCommand, Result<InputDetailResponse>>
{
    public async Task<Result<InputDetailResponse>> Handle(
        RestoreInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        if (input is null)
        {
            return Error.NotFound($"Không tìm th?y phi?u nh?p dã xóa có ID {request.Id}.", "Id");
        }
        updateRepository.Restore(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return input.Adapt<InputDetailResponse>();
    }
}

