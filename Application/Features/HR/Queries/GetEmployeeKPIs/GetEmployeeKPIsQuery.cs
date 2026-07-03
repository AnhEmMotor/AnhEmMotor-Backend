using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Queries.GetEmployeeKPIs;

public sealed record GetEmployeeKPIsQuery : IRequest<Result<List<KpiResponse>>>;
