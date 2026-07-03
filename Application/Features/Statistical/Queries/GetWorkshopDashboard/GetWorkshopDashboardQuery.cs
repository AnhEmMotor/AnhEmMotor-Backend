using Application.Features.Statistical.DTOs;
using MediatR;
using System;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public record GetWorkshopDashboardQuery(DateTimeOffset FromDate, DateTimeOffset ToDate) : IRequest<WorkshopDashboardDto>;
