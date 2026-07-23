using Application.Common.Models;
using MediatR;

namespace Application.Features.Leads.Commands.DeleteLead;

public record DeleteLeadCommand(int Id) : IRequest<Result<bool>>;
