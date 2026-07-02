using Application.ApiContracts.Logistics.Responses;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Logistics.Queries.GetFulfillmentOrders
{
    public class GetFulfillmentOrdersQuery : IRequest<List<FulfillmentOrderResponse>>
    {
        public ParcelDeliveryStatus? Status { get; set; }
        public string? Carrier { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
