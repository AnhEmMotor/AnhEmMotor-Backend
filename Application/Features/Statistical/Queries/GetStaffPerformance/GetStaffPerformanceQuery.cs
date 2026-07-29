using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Statistical.Queries.GetStaffPerformance;

public sealed record GetStaffPerformanceQuery(DateTime Start, DateTime End) : IRequest<Result<List<StaffPerformanceResponse>>>;
