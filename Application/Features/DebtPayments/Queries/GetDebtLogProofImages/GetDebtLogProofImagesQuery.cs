using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetDebtLogProofImages;

public class GetDebtLogProofImagesQuery : IRequest<Result<List<string>>>
{
    public int DebtLogId { get; set; }
}
