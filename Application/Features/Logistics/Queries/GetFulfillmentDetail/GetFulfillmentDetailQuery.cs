using Application.ApiContracts.Logistics.Responses;
using MediatR;
using System;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQuery : IRequest<FulfillmentDetailResponse>
{
    public int Id { get; set; }
}
