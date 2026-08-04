using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Commands.DeleteEmployeeKpi;

public sealed record DeleteEmployeeKpiCommand(int Id) : IRequest<Result<int>>;
