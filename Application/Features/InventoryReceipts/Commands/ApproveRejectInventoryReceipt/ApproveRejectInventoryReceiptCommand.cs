using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.ApproveRejectInventoryReceipt;

public sealed record ApproveRejectInventoryReceiptCommand(int Id, string StatusId) : IRequest<Result<InventoryReceiptDetailResponse>>;
