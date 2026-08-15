using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Sales.Returns.Commands.ProcessReturnRequest;

public class ProcessReturnRequestCommand : IRequest<Result<ReturnRequestResponse>>
{
    public int ReturnRequestId { get; set; }
    public string Status { get; set; } = string.Empty; // 'completed', 'rejected'
    public string? ReturnAction { get; set; } // 'restock', 'defect'
    public string? RejectionReason { get; set; }
    public string? Note { get; set; }
}
