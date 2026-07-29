using Application.ApiContracts.Statistical.Responses;
using MediatR;
using System;

using Application.Common.Models;

namespace Application.Features.Statistical.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(DateTime Start, DateTime End) : IRequest<Result<DashboardSummaryResponse>>;
