using Application.Common.Models;
using MediatR;

namespace Application.Features.Payments.Commands.ProcessPayOSCallback;

public sealed record ProcessPayOSCallbackCommand(long OrderCode) : IRequest<Result<int>>;
