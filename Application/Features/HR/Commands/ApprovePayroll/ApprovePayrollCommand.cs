using Application.Common.Models;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.HR.Commands.ApprovePayroll;

public record ApprovePayrollCommand(int? EmployeeId, int Month, int Year) : IRequest<Result>;

