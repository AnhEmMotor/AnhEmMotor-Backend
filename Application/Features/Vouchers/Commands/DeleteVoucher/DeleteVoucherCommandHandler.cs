using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Voucher;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vouchers.Commands.DeleteVoucher;

public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, Result<int>>
{
    private readonly IVoucherReadRepository _readRepository;
    private readonly IVoucherDeleteRepository _deleteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVoucherCommandHandler(
        IVoucherReadRepository readRepository,
        IVoucherDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork)
    {
        _readRepository = readRepository;
        _deleteRepository = deleteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (voucher == null)
        {
            return Result<int>.Failure(Error.NotFound("Voucher không tồn tại.", "Id"));
        }

        _deleteRepository.SoftDelete(voucher);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(voucher.Id);
    }
}
