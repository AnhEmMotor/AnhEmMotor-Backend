using Application.Common.Models;
using MediatR;

namespace Application.Features.OptionValues.Commands.DeleteOptionValue;

public record DeleteOptionValueCommand(int Id) : IRequest<Result>;

