using Application.ApiContracts.Statistical.Responses;
using MediatR;
using System;

namespace Application.Features.Statistical.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(DateTime Start, DateTime End) : IRequest<DashboardSummaryResponse>;
