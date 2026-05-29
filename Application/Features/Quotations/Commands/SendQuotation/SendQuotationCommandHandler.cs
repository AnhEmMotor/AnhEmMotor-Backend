using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using Mapster;
using MediatR;
using System;

namespace Application.Features.Quotations.Commands.SendQuotation
{
    public sealed class SendQuotationCommandHandler(
        IQuotationReadRepository readRepository,
        IQuotationUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<SendQuotationCommand, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            SendQuotationCommand request,
            CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (quotation is null)
            {
                return Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id");
            }
            var currentStatus = quotation.Status?.ToLower();
            if (string.Compare(currentStatus, QuotationType.Draft) != 0)
            {
                return Error.BadRequest(
                    $"Không thể gửi báo giá đang ở trạng thái '{quotation.Status}'. Chỉ cho phép gửi báo giá ở trạng thái Nháp (draft).",
                    "Status");
            }
            quotation.Status = "sent";
            updateRepository.Update(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var updated = await readRepository.GetByIdWithDetailsAsync(quotation.Id, cancellationToken)
                .ConfigureAwait(false);
            return updated!.Adapt<QuotationDetailResponse>();
        }
    }
}
