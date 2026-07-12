using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Voucher;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Vouchers.Commands.CreateVoucher;

public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, Result<int>>
{
    private readonly IVoucherReadRepository _readRepository;
    private readonly IVoucherUpdateRepository _updateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVoucherCommandHandler(
        IVoucherReadRepository readRepository,
        IVoucherUpdateRepository updateRepository,
        IUnitOfWork unitOfWork)
    {
        _readRepository = readRepository;
        _updateRepository = updateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        if (await _readRepository.ExistsByCodeAsync(req.Code, cancellationToken))
        {
            return Result<int>.Failure(Error.BadRequest($"Mã voucher '{req.Code}' đã tồn tại.", "Code"));
        }
        var voucher = new Voucher
        {
            Code = req.Code,
            Name = req.Name,
            ApplyFor = req.ApplyFor,
            Channel = req.Channel,
            Type = req.Type,
            DiscountType = req.DiscountType,
            DiscountValue = req.DiscountValue,
            MaxDiscountAmount = req.MaxDiscountAmount,
            ValidFrom = req.ValidFrom,
            ValidTo = req.ValidTo
        };
        if (req.Type == VoucherType.Private && req.AssignedCustomerIds != null)
        {
            foreach (var leadId in req.AssignedCustomerIds)
            {
                voucher.VoucherLeads.Add(new VoucherLead { LeadId = leadId });
            }
        }
        await _updateRepository.AddAsync(voucher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(voucher.Id);
    }
}
