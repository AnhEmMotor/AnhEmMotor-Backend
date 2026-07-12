using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WorkshopPayment;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries;

public class GetWorkshopPaymentDetailQueryHandler(IWorkshopPaymentReadRepository repo) : IRequestHandler<GetWorkshopPaymentDetailQuery, Result<WorkshopPaymentResponse?>>
{
    public async Task<Result<WorkshopPaymentResponse?>> Handle(GetWorkshopPaymentDetailQuery req, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result<WorkshopPaymentResponse?>.Failure(
                [Error.NotFound($"Không tìm thấy phiếu thu id={req.Id}", "Id")]);
        return Result<WorkshopPaymentResponse?>.Success(
            new WorkshopPaymentResponse
            {
                Id = entity.Id,
                PaymentNumber = entity.PaymentNumber,
                SourceType = entity.SourceType,
                SourceId = entity.SourceId,
                CustomerName = entity.CustomerName,
                CustomerPhone = entity.CustomerPhone,
                VehicleInfo = entity.VehicleInfo,
                ServiceDescription = entity.ServiceDescription,
                SubTotal = entity.SubTotal,
                DiscountAmount = entity.DiscountAmount,
                TotalAmount = entity.TotalAmount,
                PaymentMethod = entity.PaymentMethod,
                PaymentStatus = entity.PaymentStatus,
                PaidAt = entity.PaidAt,
                Notes = entity.Notes,
                InvoicePrintedAt = entity.InvoicePrintedAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            });
    }
}
