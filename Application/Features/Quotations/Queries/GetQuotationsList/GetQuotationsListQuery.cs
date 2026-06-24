using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.Quotations.Queries.GetQuotationsList;

public sealed record GetQuotationsListQuery : IRequest<Result<PagedResult<QuotationSummaryResponse?>>>
{
    public SieveModel? SieveModel { get; init; }
}
