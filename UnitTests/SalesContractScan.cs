using Application.Common.Models;
using Application.Features.SalesContracts.Commands.UpdateSalesContractStatus;
using Application.Features.SalesContracts.Commands.UploadSalesContractScan;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class SalesContractScan
{
    private readonly Mock<ISalesContractReadRepository> _readRepository = new();
    private readonly Mock<IFileInsertService> _fileInsertService = new();
    private readonly Mock<IFileReadService> _fileReadService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task UploadScan_PersistsFileUrlAndSignsApprovedContract()
    {
        var contractId = Guid.NewGuid();
        var contract = new SalesContract { Id = contractId, Status = SalesContractStatus.Approved };
        await using var fileContent = new MemoryStream([1, 2, 3]);
        _readRepository.Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        _fileInsertService
            .Setup(
                service => service.SaveFileAsIsAsync(
                    fileContent,
                    "signed-contract.pdf",
                    It.IsAny<CancellationToken>(),
                    $"sales-contracts/{contractId}"))
            .ReturnsAsync(
                Result<FileUpload>.Success(
                    new FileUpload($"sales-contracts/{contractId}/signed-contract.pdf", ".pdf", fileContent.Length)));
        _fileReadService
            .Setup(service => service.GetPublicUrl($"sales-contracts/{contractId}/signed-contract.pdf"))
            .Returns($"/api/v1/MediaFile/view-image/sales-contracts/{contractId}/signed-contract.pdf");
        var handler = new UploadSalesContractScanCommandHandler(
            _readRepository.Object,
            _fileInsertService.Object,
            _fileReadService.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new UploadSalesContractScanCommand(contractId, fileContent, "signed-contract.pdf"),
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        contract.ScannedFileUrl.Should().Be(result.Value);
        contract.Status.Should().Be(SalesContractStatus.Signed);
        contract.SignedDate.Should().NotBeNull();
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UploadScan_RejectsUnsupportedFileBeforeStorage()
    {
        await using var fileContent = new MemoryStream([1, 2, 3]);
        var handler = new UploadSalesContractScanCommandHandler(
            _readRepository.Object,
            _fileInsertService.Object,
            _fileReadService.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new UploadSalesContractScanCommand(Guid.NewGuid(), fileContent, "signed-contract.exe"),
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _fileInsertService.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UploadScan_RejectsDraftContractBeforeStorage()
    {
        var contractId = Guid.NewGuid();
        var contract = new SalesContract { Id = contractId, Status = SalesContractStatus.Draft };
        await using var fileContent = new MemoryStream([1, 2, 3]);
        _readRepository.Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        var handler = new UploadSalesContractScanCommandHandler(
            _readRepository.Object,
            _fileInsertService.Object,
            _fileReadService.Object,
            _unitOfWork.Object);
        var result = await handler.Handle(
            new UploadSalesContractScanCommand(contractId, fileContent, "signed-contract.pdf"),
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("duyệt");
        _fileInsertService.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ApproveContract_TransitionsDraftToApproved()
    {
        var contractId = Guid.NewGuid();
        var contract = new SalesContract { Id = contractId, Status = SalesContractStatus.Draft };
        _readRepository.Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        var handler = new UpdateSalesContractStatusCommandHandler(_readRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateSalesContractStatusCommand(contractId, SalesContractStatus.Approved),
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(SalesContractStatus.Approved);
        contract.SignedDate.Should().BeNull();
        _unitOfWork.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ApproveContract_RejectsSkippingApprovalAndSignature()
    {
        var contractId = Guid.NewGuid();
        var contract = new SalesContract { Id = contractId, Status = SalesContractStatus.Draft };
        _readRepository.Setup(repository => repository.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        var handler = new UpdateSalesContractStatusCommandHandler(_readRepository.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new UpdateSalesContractStatusCommand(contractId, SalesContractStatus.Fulfilled),
            CancellationToken.None)
            .ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        contract.Status.Should().Be(SalesContractStatus.Draft);
        _unitOfWork.VerifyNoOtherCalls();
    }
}
