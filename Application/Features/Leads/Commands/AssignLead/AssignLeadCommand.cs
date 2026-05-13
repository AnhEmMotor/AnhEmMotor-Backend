using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.AssignLead;

public record AssignLeadCommand(int LeadId, Guid? UserId) : IRequest<Result<int>>;

