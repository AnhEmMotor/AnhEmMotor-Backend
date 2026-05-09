using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.ResetLeads;

public record ResetLeadsCommand : IRequest<Result<bool>>;

