using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Primitives;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.WorkshopPayments.Queries;

public class GetWorkshopPaymentsListQueryHandler(IWorkshopPaymentReadRepository repo) : IRequestHandler<GetWorkshopPaymentsListQuery, Result<PagedResult<WorkshopPaymentResponse>>>
{
    public async Task<Result<PagedResult<WorkshopPaymentResponse>>> Handle(
        GetWorkshopPaymentsListQuery req,
        CancellationToken ct)
    {
        Expression<Func<global::Domain.Entities.WorkshopPayment, bool>>? filter = null;
        
        bool hasSourceType = !string.IsNullOrEmpty(req.SourceType);
        bool hasPaymentStatus = !string.IsNullOrEmpty(req.PaymentStatus);
        bool hasPaymentMethod = !string.IsNullOrEmpty(req.PaymentMethod);
        bool hasSearch = !string.IsNullOrEmpty(req.Search);
        string search = req.Search?.Trim() ?? "";

        if (hasSourceType || hasPaymentStatus || hasPaymentMethod || hasSearch)
        {
            filter = x => 
                (!hasSourceType || x.SourceType == req.SourceType) &&
                (!hasPaymentStatus || x.PaymentStatus == req.PaymentStatus) &&
                (!hasPaymentMethod || x.PaymentMethod == req.PaymentMethod) &&
                (!hasSearch || 
                    x.PaymentNumber.Contains(search) || 
                    x.CustomerName.Contains(search) || 
                    x.CustomerPhone.Contains(search) || 
                    (x.VehicleInfo != null && x.VehicleInfo.Contains(search)));
        }

        var paged = await repo.GetPagedAsync<WorkshopPaymentResponse>(req.Sieve, req.Mode, filter, ct);
        return Result<PagedResult<WorkshopPaymentResponse>>.Success(paged);
    }
}
