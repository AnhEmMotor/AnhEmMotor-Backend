using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Leads.Commands.SeedLeads;

public record SeedLeadsCommand : IRequest<Result<bool>>;


