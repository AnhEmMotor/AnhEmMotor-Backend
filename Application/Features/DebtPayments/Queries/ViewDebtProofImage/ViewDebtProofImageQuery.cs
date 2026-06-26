using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Queries.ViewDebtProofImage;

public class ViewDebtProofImageQuery : IRequest<Result<(Stream Content, string ContentType)>>
{
    public int MediaFileId { get; set; }
}
