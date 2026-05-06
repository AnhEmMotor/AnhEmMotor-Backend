using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Queries.GetPayrollSummary;

public record GetPayrollSummaryQuery(int Month, int Year) : IRequest<Result<List<PayrollDTO>>>;
