using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;

public sealed class UpdateDisbursementPaymentCommandHandler(
    IUnitOfWork unitOfWork,
    IFinanceContractReadRepository repository
) : IRequestHandler<UpdateDisbursementPaymentCommand>
{
    public async Task Handle(UpdateDisbursementPaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.FinanceContractId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng tài chính với Id = {request.FinanceContractId}");
        }
        if (entity.DisbursementStatus != "Pending")
        {
            throw new InvalidOperationException(
                $"Chỉ cập nhật giải ngân được khi trạng thái là Pending. Hiện tại: {entity.DisbursementStatus}");
        }
        entity.DisbursementStatus = "Disbursed";
        entity.SignedDate = request.Request.ActualDate;
        if (request.Request.ActualAmount != entity.LoanAmount)
        {
            throw new InvalidOperationException(
                $"Số tiền giải ngân thực tế ({request.Request.ActualAmount}) không khớp với giá trị hợp đồng ({entity.LoanAmount}). " +
                    "Entity hiện chưa có trường lưu ActualAmount riêng, nên yêu cầu ActualAmount phải bằng LoanAmount.");
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
