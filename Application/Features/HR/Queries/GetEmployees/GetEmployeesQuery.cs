using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.HR.Queries.GetEmployees;

public record GetEmployeesQuery : IRequest<Result<List<EmployeeResponse>>>;


