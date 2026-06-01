using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt
{
    public sealed class CloneInventoryReceiptCommandHandler : IRequestHandler<CloneInventoryReceiptCommand, Result<InventoryReceiptDetailResponse?>>
    {
        public Task<Result<InventoryReceiptDetailResponse?>> Handle(
            CloneInventoryReceiptCommand command,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}