using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(
            request.Id,
            cancellationToken)
            .ConfigureAwait(false);

        if (input is null)
        {
            throw new InvalidOperationException($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }

        deleteRepository.Delete(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
