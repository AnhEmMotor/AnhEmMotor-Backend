using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyById;

public record GetCommissionPolicyByIdQuery(int Id) : IRequest<Result<CommissionPolicy>>;
