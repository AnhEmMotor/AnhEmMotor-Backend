using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WorkshopPayments.Commands.CreateWorkshopPayment;

public class CreateWorkshopPaymentCommandHandler : IRequestHandler<CreateWorkshopPaymentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWorkshopPaymentCommand request, CancellationToken cancellationToken)
    {
        // Mock implementation
        await Task.CompletedTask;
        return Result<int>.Success(1);
    }
}
