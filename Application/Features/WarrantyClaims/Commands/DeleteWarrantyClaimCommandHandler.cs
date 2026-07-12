using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public class DeleteWarrantyClaimCommandHandler(
    IWarrantyClaimReadRepository readRepo,
    IWarrantyClaimWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<DeleteWarrantyClaimCommand, Result>
{
    public async Task<Result> Handle(DeleteWarrantyClaimCommand req, CancellationToken ct)
    {
        var claim = await readRepo.GetByIdAsync(req.Id, ct);
        if (claim is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={req.Id}", "Id")]);
        writeRepo.Delete(claim);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
