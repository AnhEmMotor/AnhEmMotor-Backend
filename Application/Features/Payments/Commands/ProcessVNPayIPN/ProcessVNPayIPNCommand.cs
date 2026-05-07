using Application.ApiContracts.Payment.Responses;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Payments.Commands.ProcessVNPayIPN;

public sealed record ProcessVNPayIPNCommand(IQueryCollection Query) : IRequest<Result<VNPayPaymentResponse>>;

