using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public record GetCommissionPoliciesQuery : IRequest<List<CommissionPolicy>>;
