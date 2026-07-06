using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Voucher;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Vouchers.Commands.UpdateVoucher;

public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, Result<int>>
{
    private readonly IVoucherReadRepository _readRepository;
    private readonly IVoucherUpdateRepository _updateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVoucherCommandHandler(
        IVoucherReadRepository readRepository,
        IVoucherUpdateRepository updateRepository,
        IUnitOfWork unitOfWork)
    {
        _readRepository = readRepository;
        _updateRepository = updateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var voucher = await _readRepository.GetByIdAsync(req.Id, cancellationToken);

        if (voucher == null)
        {
            return Result<int>.Failure(Error.NotFound("Voucher không tồn tại.", "Id"));
        }

        if (await _readRepository.ExistsByCodeAsync(req.Code, req.Id, cancellationToken))
        {
            return Result<int>.Failure(Error.BadRequest($"Mã voucher '{req.Code}' đã tồn tại.", "Code"));
        }

        voucher.Code = req.Code;
        voucher.Name = req.Name;
        voucher.ApplyFor = req.ApplyFor;
        voucher.Channel = req.Channel;
        voucher.Type = req.Type;
        voucher.DiscountType = req.DiscountType;
        voucher.DiscountValue = req.DiscountValue;
        voucher.MaxDiscountAmount = req.MaxDiscountAmount;
        voucher.ValidFrom = req.ValidFrom;
        voucher.ValidTo = req.ValidTo;

        voucher.VoucherLeads.Clear();

        if (req.Type == Domain.Enums.VoucherType.Private && req.AssignedCustomerIds != null)
        {
            foreach (var leadId in req.AssignedCustomerIds)
            {
                voucher.VoucherLeads.Add(new VoucherLead
                {
                    LeadId = leadId
                });
            }
        }

        _updateRepository.Update(voucher);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(voucher.Id);
    }
}
