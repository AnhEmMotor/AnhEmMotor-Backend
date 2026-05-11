using Domain.Entities;
using Domain.Entities.HR;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public record GetCommissionPoliciesQuery : IRequest<List<CommissionPolicy>>;
