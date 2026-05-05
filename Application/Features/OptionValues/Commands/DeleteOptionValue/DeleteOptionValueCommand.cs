using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;

namespace Application.Features.OptionValues.Commands.DeleteOptionValue;

public record DeleteOptionValueCommand(int Id) : IRequest<Result>;


