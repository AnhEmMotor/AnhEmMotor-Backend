using Application.Common.Models;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Leads.Commands.SeedLeads;

public record SeedLeadsCommand : IRequest<Result<bool>>;

