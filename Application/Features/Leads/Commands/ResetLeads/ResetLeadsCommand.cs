using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.ResetLeads;

public record ResetLeadsCommand : IRequest<Result<bool>>;

