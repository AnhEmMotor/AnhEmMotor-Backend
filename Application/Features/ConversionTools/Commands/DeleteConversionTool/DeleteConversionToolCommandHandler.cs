using Application.Common.Models;
using Application.Features.ConversionTools.Commands.DeleteConversionTool;
using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversionTools.Commands.DeleteConversionTool;

public class DeleteConversionToolCommandHandler(IConversionToolReadRepository readRepo, IConversionToolWriteRepository writeRepo)
    : IRequestHandler<DeleteConversionToolCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteConversionToolCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return Result<bool>.Failure("Không tìm thấy công cụ chuyển đổi");

        await writeRepo.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await writeRepo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<bool>.Success(true);
    }
}
