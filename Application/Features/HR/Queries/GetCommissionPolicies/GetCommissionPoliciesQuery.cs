using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public record GetCommissionPoliciesQuery : IRequest<Result<List<CommissionPolicy>>>;

