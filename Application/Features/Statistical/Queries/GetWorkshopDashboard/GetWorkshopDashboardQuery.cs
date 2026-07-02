using MediatR;
using System;
using Application.Features.Statistical.DTOs;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public record GetWorkshopDashboardQuery(DateTimeOffset FromDate, DateTimeOffset ToDate) : IRequest<WorkshopDashboardDto>;
