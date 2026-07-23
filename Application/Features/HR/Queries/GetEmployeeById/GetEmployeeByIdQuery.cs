using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(int Id) : IRequest<Result<EmployeeResponse>>;
