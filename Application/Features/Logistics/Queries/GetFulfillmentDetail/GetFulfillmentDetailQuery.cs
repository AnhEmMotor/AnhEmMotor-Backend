using System;
using System.Collections.Generic;
using Application.ApiContracts.Logistics.Responses;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQuery : IRequest<FulfillmentDetailResponse>
{
    public int Id { get; set; }
}
