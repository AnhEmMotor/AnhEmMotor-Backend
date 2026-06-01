using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Application.Interfaces.Services;
using Domain.Constants;
using Mapster;
using MediatR;
using System;

namespace Application.Features.Quotations.Commands.ApproveRejectQuotation
{
    public sealed class ApproveRejectQuotationCommandHandler(
        IQuotationReadRepository readRepository,
        IQuotationUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext? currentUserContext = null) : IRequestHandler<ApproveRejectQuotationCommand, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            ApproveRejectQuotationCommand request,
            CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (quotation is null)
            {
                return Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id");
            }
            if (!string.Equals(request.Status, QuotationType.Approved, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Status, QuotationType.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Trạng thái phê duyệt không hợp lệ.", "Status");
            }
            var currentStatus = quotation.Status?.ToLower();
            if (string.Compare(currentStatus, QuotationType.Sent) != 0)
            {
                return Error.BadRequest(
                    $"Không thể cập nhật trạng thái báo giá đang ở trạng thái '{quotation.Status}'. Chỉ cho phép cập nhật báo giá ở trạng thái Đã gửi (sent).",
                    "Status");
            }
            quotation.Status = request.Status.ToLower();
            var currentUserId = currentUserContext?.GetUserId();
            if (string.Equals(quotation.Status, QuotationType.Approved, StringComparison.OrdinalIgnoreCase))
            {
                quotation.ApprovedBy = currentUserId;
                quotation.RejectedBy = null;
            }
            else if (string.Equals(quotation.Status, QuotationType.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                quotation.RejectedBy = currentUserId;
                quotation.ApprovedBy = null;
            }
            updateRepository.Update(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var updated = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken)
                .ConfigureAwait(false);
            return updated!.Adapt<QuotationDetailResponse>();
        }
    }
}
