using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Commands.ApproveQuotation
{
    public sealed class ApproveQuotationCommandHandler(
        IQuotationReadRepository readRepository,
        IQuotationUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<ApproveQuotationCommand, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            ApproveQuotationCommand request,
            CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (quotation is null)
            {
                return Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id");
            }

            var currentStatus = quotation.Status?.ToLower();
            if (currentStatus != "sent")
            {
                return Error.BadRequest($"Không thể xác nhận báo giá đang ở trạng thái '{quotation.Status}'. Chỉ cho phép xác nhận báo giá ở trạng thái Đã gửi (sent).", "Status");
            }

            quotation.Status = "approved";
            updateRepository.Update(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<QuotationDetailResponse>();
        }
    }
}
