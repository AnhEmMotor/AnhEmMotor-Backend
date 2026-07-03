using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Sales.Returns.Queries.GetReturnRequests;

public class GetReturnRequestsQuery : IRequest<Result<PagedResult<ReturnRequestResponse>>>
{
    public SieveModel SieveModel { get; set; } = new();
}

public class GetReturnRequestsQueryHandler : IRequestHandler<GetReturnRequestsQuery, Result<PagedResult<ReturnRequestResponse>>>
{
    private readonly IReturnRequestReadRepository _repository;

    public GetReturnRequestsQueryHandler(IReturnRequestReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<ReturnRequestResponse>>> Handle(GetReturnRequestsQuery request, CancellationToken cancellationToken)
    {
        var pagedEntities = await _repository.GetPagedAsync(request.SieveModel, cancellationToken);

        var items = pagedEntities.Items?.Select(p => new ReturnRequestResponse
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
        }).ToList();

        var pagedResult = new PagedResult<ReturnRequestResponse>(items, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);

        return Result<PagedResult<ReturnRequestResponse>>.Success(pagedResult);
    }
}
