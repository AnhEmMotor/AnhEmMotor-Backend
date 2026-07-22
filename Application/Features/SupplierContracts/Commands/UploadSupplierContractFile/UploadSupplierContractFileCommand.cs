using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.UploadSupplierContractFile;

public sealed record UploadSupplierContractFileCommand(
    Guid ContractId,
    Stream FileContent,
    string FileName) : IRequest<Result<string>>;
