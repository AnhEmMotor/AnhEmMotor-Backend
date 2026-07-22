using Application.Common.Models;
using Application.Features.SupplierContracts.Commands.UploadSupplierContractFile;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class SupplierContractFileUpload
{
    private readonly Mock<ISupplierContractReadRepository> _readRepository = new();
    private readonly Mock<IFileInsertService> _fileInsertService = new();
    private readonly Mock<IFileReadService> _fileReadService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task UploadFile_PersistsPublicUrlOnSupplierContract()
    {
        var contractId = Guid.NewGuid();
        var contract = new SupplierContract { Id = contractId, Status = "Draft" };
        await using var fileContent = new MemoryStream([1, 2, 3]);
        _readRepository
            .Setup(repository => repository.GetByIdAsync(
                contractId,
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(contract);
        _fileInsertService
            .Setup(service => service.SaveFileAsIsAsync(
                fileContent,
                "supplier-contract.docx",
                It.IsAny<CancellationToken>(),
                $"supplier-contracts/{contractId}"))
            .ReturnsAsync(Result<FileUpload>.Success(
                new FileUpload(
                    $"supplier-contracts/{contractId}/supplier-contract.docx",
                    ".docx",
                    fileContent.Length)));
        _fileReadService
            .Setup(service => service.GetPublicUrl(
                $"supplier-contracts/{contractId}/supplier-contract.docx"))
            .Returns($"/api/v1/MediaFile/view-image/supplier-contracts/{contractId}/supplier-contract.docx");
        var handler = new UploadSupplierContractFileCommandHandler(
            _readRepository.Object,
            _fileInsertService.Object,
            _fileReadService.Object,
            _unitOfWork.Object);

        var result = await handler.Handle(
            new UploadSupplierContractFileCommand(
                contractId,
                fileContent,
                "supplier-contract.docx"),
            CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        contract.ContractFilePath.Should().Be(result.Value);
        contract.UpdatedAt.Should().NotBeNull();
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UploadFile_RejectsUnsupportedFileBeforeStorage()
    {
        await using var fileContent = new MemoryStream([1, 2, 3]);
        var handler = new UploadSupplierContractFileCommandHandler(
            _readRepository.Object,
            _fileInsertService.Object,
            _fileReadService.Object,
            _unitOfWork.Object);

        var result = await handler.Handle(
            new UploadSupplierContractFileCommand(
                Guid.NewGuid(),
                fileContent,
                "supplier-contract.exe"),
            CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        _fileInsertService.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }
}
