using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.WorkshopPayments.Queries;

public class GetWorkshopPaymentsListQuery : IRequest<Result<PagedResult<WorkshopPaymentResponse>>>
{
    public SieveModel Sieve { get; set; } = new();

    public DataFetchMode Mode { get; set; } = DataFetchMode.ActiveOnly;

    public string? SourceType { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Search { get; set; }
}
