using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Commands.DeleteEmployee;

public sealed record DeleteEmployeeCommand(int Id) : IRequest<Result>;
