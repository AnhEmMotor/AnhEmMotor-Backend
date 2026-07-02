using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ReturnRequest;
using MediatR;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Sales.Returns.Queries.GetReturnRequestDetail;

public class GetReturnRequestDetailQuery : IRequest<Result<ReturnRequestResponse>>
{
    public int Id { get; set; }
}

public class GetReturnRequestDetailQueryHandler : IRequestHandler<GetReturnRequestDetailQuery, Result<ReturnRequestResponse>>
{
    private readonly IReturnRequestReadRepository _repository;

    public GetReturnRequestDetailQueryHandler(IReturnRequestReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ReturnRequestResponse>> Handle(GetReturnRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var p = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (p == null) return Result<ReturnRequestResponse>.Failure("Not found");

        var response = new ReturnRequestResponse
        {
            Id = p.Id,
            OrderId = p.OrderId,
            OrderCode = p.OrderCode,
            OriginalTrackingNumber = p.OriginalTrackingNumber,
            CustomerName = p.CustomerName,
            CustomerPhone = p.CustomerPhone,
            Carrier = p.Carrier,
            Type = p.Type,
            Status = p.Status,
            Reason = p.Reason,
            CancelReason = p.CancelReason,
            Note = p.Note,
            ReturnAction = p.ReturnAction,
            RejectionReason = p.RejectionReason,
            CreatedAt = p.CreatedAt ?? System.DateTimeOffset.UtcNow,
            InspectedAt = p.InspectedAt,
            Items = p.Items.Select(i => new ReturnRequestItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                ThumbnailUrl = i.ThumbnailUrl,
                Quantity = i.Quantity,
                ReturnQuantity = i.ReturnQuantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        return Result<ReturnRequestResponse>.Success(response);
    }
}
