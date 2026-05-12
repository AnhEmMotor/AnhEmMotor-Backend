using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public record GetCommissionPoliciesQuery : IRequest<List<CommissionPolicy>>;
