using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public record GetPayrollSummaryQuery(int Month, int Year) : IRequest<Result<List<PayrollDTO>>>;
