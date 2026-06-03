using Application.ApiContracts.FinanceContract.Requests;
using MediatR;

namespace Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;

public sealed record UpdateDisbursementPaymentCommand(Guid FinanceContractId, UpdateDisbursementPaymentRequest Request, Guid CurrentUserId)
    : IRequest;

