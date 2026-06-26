using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.DebtPayments.Commands.UploadDebtProofImage;

public class UploadDebtProofImageCommand : IRequest<Result<UploadDebtProofImageResponse>>
{
    public Stream FileContent { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
