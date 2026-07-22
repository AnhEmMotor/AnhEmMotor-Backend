using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UploadSalesContractScan;

public sealed record UploadSalesContractScanCommand(
    Guid ContractId,
    Stream FileContent,
    string FileName) : IRequest<Result<string>>;
