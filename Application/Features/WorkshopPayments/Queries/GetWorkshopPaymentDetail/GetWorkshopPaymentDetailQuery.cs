using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentDetail;

public class GetWorkshopPaymentDetailQuery : IRequest<Result<object>>
{
    public int Id { get; set; }
}
