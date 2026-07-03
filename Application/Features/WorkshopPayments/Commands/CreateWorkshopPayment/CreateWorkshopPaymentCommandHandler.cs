using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.WorkshopPayments.Commands.CreateWorkshopPayment;

public class CreateWorkshopPaymentCommandHandler : IRequestHandler<CreateWorkshopPaymentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWorkshopPaymentCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<int>.Success(1);
    }
}
