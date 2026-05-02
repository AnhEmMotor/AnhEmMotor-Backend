using Application.ApiContracts.Payment.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Payments.Commands.ProcessPayOSWebhook;

public sealed record ProcessPayOSWebhookCommand(PayOSWebhookData Data) : IRequest<Result>;
