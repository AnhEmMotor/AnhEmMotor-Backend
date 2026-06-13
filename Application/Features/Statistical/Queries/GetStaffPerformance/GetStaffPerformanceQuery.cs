using Application.ApiContracts.Statistical.Responses;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Statistical.Queries.GetStaffPerformance;

public sealed record GetStaffPerformanceQuery(DateTime Start, DateTime End) : IRequest<List<StaffPerformanceResponse>>;
