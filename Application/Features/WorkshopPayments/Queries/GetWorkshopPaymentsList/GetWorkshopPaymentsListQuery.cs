using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentsList;

public class GetWorkshopPaymentsListQuery : IRequest<Result<PagedResult<object>>>
{
    public SieveModel SieveModel { get; set; } = new SieveModel();
}
