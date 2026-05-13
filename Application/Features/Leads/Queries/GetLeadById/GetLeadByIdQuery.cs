using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using MediatR;
using System.Linq;

namespace Application.Features.Leads.Queries.GetLeadById;

public record GetLeadByIdQuery(int Id) : IRequest<Result<LeadResponse>>;

