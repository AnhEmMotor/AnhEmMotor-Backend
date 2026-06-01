using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus
{
    public sealed class UpdateInventoryReceiptStatusCommandHandler : IRequestHandler<UpdateInventoryReceiptStatusCommand, Result<InventoryReceiptDetailResponse>>
    {
        public Task<Result<InventoryReceiptDetailResponse>> Handle(
            UpdateInventoryReceiptStatusCommand request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
