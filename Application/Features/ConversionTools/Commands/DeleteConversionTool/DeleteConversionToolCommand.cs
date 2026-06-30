using Application.Common.Models;
using MediatR;

namespace Application.Features.ConversionTools.Commands.DeleteConversionTool;

public record DeleteConversionToolCommand(int Id) : IRequest<Result<bool>>;
