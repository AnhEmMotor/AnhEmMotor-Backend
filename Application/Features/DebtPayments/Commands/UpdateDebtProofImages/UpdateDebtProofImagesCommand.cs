using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Commands.UpdateDebtProofImages
{
    public class UpdateDebtProofImagesCommand : IRequest<Result<bool>>
    {
        public int DebtLogId { get; set; }

        public List<string> ProofImageUrls { get; set; } = new();
    }
}
