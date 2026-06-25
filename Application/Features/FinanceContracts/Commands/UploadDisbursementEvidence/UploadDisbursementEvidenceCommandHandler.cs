using Application.Interfaces.Repositories.FinanceContract;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;

public sealed class UploadDisbursementEvidenceCommandHandler(
    IFinanceContractReadRepository repository
) : IRequestHandler<UploadDisbursementEvidenceCommand>
{
    public async Task Handle(UploadDisbursementEvidenceCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.FinanceContractId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng tài chính với Id = {request.FinanceContractId}");
        }
        throw new NotSupportedException(
            "Tải lên chứng từ giải ngân chưa được hỗ trợ: FinanceContract entity hiện chưa có trường lưu URL minh chứng. Cần thêm cột DisbursementEvidenceUrl trước khi triển khai tính năng này.");
    }
}
