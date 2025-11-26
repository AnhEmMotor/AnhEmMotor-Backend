using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public sealed class DeleteOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOutputCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteOutputCommand request,
        CancellationToken cancellationToken)
    {
        var output = await readRepository.GetByIdAsync(
            request.Id,
            cancellationToken)
            .ConfigureAwait(false);

        if (output is null)
        {
            throw new InvalidOperationException($"Không tìm thấy đơn hàng có ID {request.Id}.");
        }

        deleteRepository.Delete(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
