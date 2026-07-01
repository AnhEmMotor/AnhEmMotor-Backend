using Application.Common.Models;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Primitives;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentsList;

public class GetWorkshopPaymentsListQueryHandler : IRequestHandler<GetWorkshopPaymentsListQuery, Result<PagedResult<object>>>
{
    private readonly IWorkshopPaymentReadRepository _repository;

    public GetWorkshopPaymentsListQueryHandler(IWorkshopPaymentReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<object>>> Handle(GetWorkshopPaymentsListQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.SieveModel, cancellationToken);

        var items = pagedEntities.Items?.Select(p => (object)new 
        {
            id = p.Id,
            paymentNumber = p.PaymentNumber,
            sourceType = p.SourceType,
            sourceId = p.SourceId,
            customerName = p.CustomerName,
            customerPhone = p.CustomerPhone,
            vehicleInfo = p.VehicleInfo,
            serviceDescription = p.ServiceDescription,
            subTotal = p.SubTotal,
            discountAmount = p.DiscountAmount,
            totalAmount = p.TotalAmount,
            paymentMethod = p.PaymentMethod,
            paymentStatus = p.PaymentStatus,
            receivedById = p.ReceivedById,
            paidAt = p.PaidAt,
            notes = p.Notes,
            invoicePrintedAt = p.InvoicePrintedAt,
            createdAt = p.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<object>(items, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);
        return Result<PagedResult<object>>.Success(pagedResult);
    }
}
