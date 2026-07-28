using Microsoft.AspNetCore.Http;
using Application.ApiContracts.File.Requests;
using Application.Common.Models;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreManyFiles;
using Application.Features.Files.Commands.UploadManyProductImages;
using Application.Features.Files.Commands.UploadProductImage;
using Application.Features.Files.Queries.GetFileById;
using Application.Features.Files.Queries.ViewImage;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Domain.Constants;
using FluentAssertions;
using Infrastructure.Configurations.Options;
using Infrastructure.Repositories.MediaFile.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace UnitTests;

public class MediaFile
{
    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    private readonly Mock<IFileReadService> _fileReadServiceMock;
    private readonly Mock<IFileInsertService> _fileInsertServiceMock;
    private readonly Mock<IFileUpdateService> _fileUpdateServiceMock;
private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IFileDeleteService> _fileDeleteServiceMock;
    private readonly Mock<IMediaFileInsertRepository> _insertRepositoryMock;
    private readonly Mock<IMediaFileReadRepository> _readRepositoryMock;
    private readonly Mock<IMediaFileUpdateRepository> _updateRepositoryMock;
    private readonly Mock<IMediaFileDeleteRepository> _deleteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public MediaFile()
    {
        _fileReadServiceMock = new Mock<IFileReadService>();
        _fileInsertServiceMock = new Mock<IFileInsertService>();
        _fileUpdateServiceMock = new Mock<IFileUpdateService>();
_httpContextAccessorMock = new Mock<IHttpContextAccessor>();
_fileDeleteServiceMock = new Mock<IFileDeleteService>();
        _insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        _readRepositoryMock = new Mock<IMediaFileReadRepository>();
        _updateRepositoryMock = new Mock<IMediaFileUpdateRepository>();
        _deleteRepositoryMock = new Mock<IMediaFileDeleteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "MF_001 - Táº£i lÃªn áº£nh thÃ nh cÃ´ng vá»›i Ä‘á»‹nh dáº¡ng WEBP há»£p lá»‡")]
    public async Task UploadImage_ValidWebp_Success()
    {
        var expectedStoragePath = "test-guid-123.webp";
        var expectedFileExtension = ".webp";
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream = new MemoryStream(new byte[102400]);
        var command = new UploadProductImageCommand { FileContent = stream, FileName = "test.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);
        result.Value.OriginalFileName.Should().Be("test.webp");
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once());
        _insertRepositoryMock.Verify(x => x.Add(It.IsAny<MediaFileEntity>()), Times.Once());
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_003 - Táº£i lÃªn áº£nh tháº¥t báº¡i vá»›i Ä‘á»‹nh dáº¡ng khÃ´ng Ä‘Æ°á»£c há»— trá»£")]
    public async Task UploadImage_UnsupportedFormat_Fail()
    {
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Failure("Unsupported file format"));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var pdfStream = new MemoryStream(new byte[1024]);
        var pdfCommand = new UploadProductImageCommand { FileContent = pdfStream, FileName = "document.pdf" };
        var result = await handler.Handle(pdfCommand, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        var txtStream = new MemoryStream(new byte[512]);
        var txtCommand = new UploadProductImageCommand { FileContent = txtStream, FileName = "text.txt" };
        result = await handler.Handle(txtCommand, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        var fakeWebpStream = new MemoryStream(new byte[1024]);
        var fakeWebpCommand = new UploadProductImageCommand { FileContent = fakeWebpStream, FileName = "fake.webp" };
        result = await handler.Handle(fakeWebpCommand, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_004 - Táº£i lÃªn áº£nh tháº¥t báº¡i khi kÃ­ch thÆ°á»›c file vÆ°á»£t quÃ¡ giá»›i háº¡n")]
    public async Task UploadImage_FileSizeExceedsLimit_Fail()
    {
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var largeStream = new MemoryStream(new byte[52428800]);
        var command = new UploadProductImageCommand { FileContent = largeStream, FileName = "large.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_008 - Táº£i lÃªn nhiá»u áº£nh tháº¥t báº¡i khi cÃ³ 1 file khÃ´ng há»£p lá»‡ (Bulk Request Rule)")]
    public async Task UploadManyImages_OneInvalidFile_FailAll()
    {
        var handler = new UploadManyProductImagesCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var files = new List<FileParameter>
        {
            new FileParameter { Content = new MemoryStream(new byte[51200]), FileName = "valid1.webp" },
            new FileParameter { Content = new MemoryStream(new byte[102400]), FileName = "invalid.pdf" },
            new FileParameter { Content = new MemoryStream(new byte[61440]), FileName = "valid2.jpg" }
        };
        var command = new UploadManyProductImagesCommand { Files = files };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Never());
    }

    [Fact(DisplayName = "MF_010 - XoÃ¡ file tháº¥t báº¡i khi file khÃ´ng tá»“n táº¡i")]
    public async Task DeleteFile_FileNotFound_Fail()
    {
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathAsync(
                "nonexistent-file.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new DeleteProductImageCommandHandler(
            _readRepositoryMock.Object,
            _deleteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileDeleteServiceMock.Object);
        var command = new DeleteProductImageCommand { StoragePath = "nonexistent-file.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_011 - XoÃ¡ file tháº¥t báº¡i khi file Ä‘Ã£ bá»‹ xoÃ¡ trÆ°á»›c Ä‘Ã³")]
    public async Task DeleteFile_AlreadyDeleted_Fail()
    {
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathAsync(
                "already-deleted.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new DeleteProductImageCommandHandler(
            _readRepositoryMock.Object,
            _deleteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileDeleteServiceMock.Object);
        var command = new DeleteProductImageCommand { StoragePath = "already-deleted.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_014 - XoÃ¡ nhiá»u file tháº¥t báº¡i khi cÃ³ 1 file khÃ´ng tá»“n táº¡i (Bulk Request Rule)")]
    public async Task DeleteManyFiles_OneNotFound_FailAll()
    {
        var existingFiles = new List<MediaFileEntity>
        {
            new() { StoragePath = "exist1.webp", DeletedAt = null },
            new() { StoragePath = "exist2.webp", DeletedAt = null }
        };
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync(existingFiles);
        var handler = new DeleteManyFilesCommandHandler(
            _readRepositoryMock.Object,
            _deleteRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteManyFilesCommand { StoragePaths = ["exist1.webp", "nonexistent.jpg", "exist2.webp"] };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<IEnumerable<MediaFileEntity>>()), Times.Never());
    }

    [Fact(DisplayName = "MF_016 - KhÃ´i phá»¥c file tháº¥t báº¡i khi file khÃ´ng tá»“n táº¡i")]
    public async Task RestoreFile_FileNotFound_Fail()
    {
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathAsync(
                "nonexistent-file.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new RestoreFileCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _fileReadServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreFileCommand { StoragePath = "nonexistent-file.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _updateRepositoryMock.Verify(x => x.Update(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_017 - KhÃ´i phá»¥c file tháº¥t báº¡i khi file chÆ°a bá»‹ xoÃ¡")]
    public async Task RestoreFile_FileNotDeleted_Fail()
    {
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathAsync("active-file.webp", It.IsAny<CancellationToken>(), DataFetchMode.DeletedOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new RestoreFileCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _fileReadServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreFileCommand { StoragePath = "active-file.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _updateRepositoryMock.Verify(x => x.Update(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_020 - KhÃ´i phá»¥c nhiá»u file tháº¥t báº¡i khi cÃ³ 1 file khÃ´ng tá»“n táº¡i (Bulk Request Rule)")]
    public async Task RestoreManyFiles_OneNotFound_FailAll()
    {
        var deletedFiles = new List<MediaFileEntity>
        {
            new() { StoragePath = "deleted1.webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") },
            new() { StoragePath = "deleted2.webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") }
        };
        _readRepositoryMock.Setup(
            x => x.GetByStoragePathsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync(deletedFiles);
        var handler = new RestoreManyFilesCommandHandler(
            _readRepositoryMock.Object,
            _updateRepositoryMock.Object,
            _fileReadServiceMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreManyFilesCommand
        {
            StoragePaths = ["deleted1.webp", "nonexistent.jpg", "deleted2.webp"]
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _updateRepositoryMock.Verify(x => x.Restore(It.IsAny<IEnumerable<MediaFileEntity>>()), Times.Never());
    }

    [Fact(DisplayName = "MF_023 - Xem áº£nh tháº¥t báº¡i khi file khÃ´ng tá»“n táº¡i")]
    public async Task ViewImage_FileNotFound_Fail()
    {
        _fileReadServiceMock.Setup(x => x.GetFileAsync("nonexistent.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "nonexistent.webp" };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_024 - Xem áº£nh tháº¥t báº¡i khi file Ä‘Ã£ bá»‹ xoÃ¡")]
    public async Task ViewImage_FileDeleted_Fail()
    {
        _fileReadServiceMock.Setup(x => x.GetFileAsync("deleted-image.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "deleted-image.webp" };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_025 - Xem áº£nh tháº¥t báº¡i khi width lÃ  sá»‘ Ã¢m")]
    public async Task ViewImage_NegativeWidth_Fail()
    {
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "test.webp", Width = -100 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_026 - Xem áº£nh tháº¥t báº¡i khi width vÆ°á»£t quÃ¡ giá»›i háº¡n cho phÃ©p")]
    public async Task ViewImage_WidthExceedsLimit_Fail()
    {
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "test.webp", Width = 50000 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_026_NonImage - Xem tá»‡p khÃ´ng pháº£i lÃ  áº£nh (PDF) tráº£ vá» trá»±c tiáº¿p luá»“ng dá»¯ liá»‡u thÃ´")]
    public async Task ViewImage_NonImageFile_ReturnsRawStream()
    {
        var rawBytes = new byte[] { 1, 2, 3, 4 };
        _fileReadServiceMock.Setup(x => x.GetFileAsync("doc.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((rawBytes, "application/pdf"));
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "doc.pdf" };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContentType.Should().Be("application/pdf");
        var outputStream = result.Value.FileStream;
        var outputBytes = new byte[4];
        await outputStream.ReadExactlyAsync(outputBytes, CancellationToken.None);
        outputBytes.Should().Equal(rawBytes);
        _fileReadServiceMock.Verify(
            x => x.ReadImageAsync(It.IsAny<Stream>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "MF_026_Fallback - Xem áº£nh lá»—i Ä‘á»‹nh dáº¡ng tráº£ vá» trá»±c tiáº¿p luá»“ng dá»¯ liá»‡u thÃ´ cá»§a file")]
    public async Task ViewImage_UnknownImageFormat_ReturnsRawStreamFallback()
    {
        var rawBytes = new byte[] { 5, 6, 7, 8 };
        _fileReadServiceMock.Setup(x => x.GetFileAsync("fake_image.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync((rawBytes, "image/png"));
        _fileReadServiceMock.Setup(
            x => x.ReadImageAsync(It.IsAny<Stream>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnknownImageFormatException("Unknown format"));
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "fake_image.png", Width = 300 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ContentType.Should().Be("image/png");
        var outputStream = result.Value.FileStream;
        var outputBytes = new byte[4];
        await outputStream.ReadExactlyAsync(outputBytes, CancellationToken.None);
        outputBytes.Should().Equal(rawBytes);
    }

    [Fact(DisplayName = "MF_027 - Validate: TÃªn file gá»‘c chá»©a kÃ½ tá»± Ä‘áº·c biá»‡t")]
    public async Task UploadImage_FileNameWithSpecialChars_Success()
    {
        var expectedStoragePath = "test-guid-456.webp";
        var expectedFileExtension = ".webp";
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadProductImageCommand { FileContent = stream, FileName = "test<>:\"/\\|?*.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().NotBeNullOrEmpty();
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once());
    }

    [Fact(DisplayName = "MF_028 - Validate: TÃªn file gá»‘c cÃ³ khoáº£ng tráº¯ng Ä‘áº§u cuá»‘i")]
    public async Task UploadImage_FileNameWithWhitespace_Success()
    {
        var expectedStoragePath = "test-guid-789.webp";
        var expectedFileExtension = ".webp";
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadProductImageCommand { FileContent = stream, FileName = "  test image.webp  " };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().NotBeNullOrEmpty();
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once());
    }

    [Fact(DisplayName = "MF_029 - Security: File signature khÃ´ng khá»›p vá»›i extension (webp fake)")]
    public async Task UploadImage_WebpFakeSignature_Fail()
    {
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Failure("Invalid file signature"));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var fakeStream = new MemoryStream(new byte[51200]);
        var command = new UploadProductImageCommand { FileContent = fakeStream, FileName = "fake.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
        _insertRepositoryMock.Verify(x => x.Add(It.IsAny<MediaFileEntity>()), Times.Never);
    }

    [Fact(DisplayName = "MF_030 - Security: File signature khÃ´ng khá»›p vá»›i extension (jpg fake)")]
    public async Task UploadImage_JpgFakeSignature_Fail()
    {
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Failure("Invalid file signature"));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var fakeStream = new MemoryStream(new byte[51200]);
        var command = new UploadProductImageCommand { FileContent = fakeStream, FileName = "fake.jpg" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once);
        _insertRepositoryMock.Verify(x => x.Add(It.IsAny<MediaFileEntity>()), Times.Never);
    }

    [Fact(DisplayName = "MF_032 - Láº¥y thÃ´ng tin file theo ID tháº¥t báº¡i khi file khÃ´ng tá»“n táº¡i")]
    public async Task GetFileById_FileNotFound_Fail()
    {
        _readRepositoryMock.Setup(x => x.GetByIdAsync(999999, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new GetFileByIdQueryHandler(_readRepositoryMock.Object, _fileReadServiceMock.Object);
        var query = new GetFileByIdQuery { Id = 999999 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_033 - Láº¥y thÃ´ng tin file theo ID tháº¥t báº¡i khi file Ä‘Ã£ bá»‹ xoÃ¡")]
    public async Task GetFileById_FileDeleted_Fail()
    {
        _readRepositoryMock.Setup(x => x.GetByIdAsync(456, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new GetFileByIdQueryHandler(_readRepositoryMock.Object, _fileReadServiceMock.Object);
        var query = new GetFileByIdQuery { Id = 456 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_040 - StorageType validation: Kiá»ƒm tra giÃ¡ trá»‹ há»£p lá»‡")]
    public async Task UploadImage_ValidStorageType_Success()
    {
        var expectedStoragePath = "test-guid-999.webp";
        var expectedFileExtension = ".webp";
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadProductImageCommand { FileContent = stream, FileName = "test.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once());
    }

    [Fact(DisplayName = "MF_043 - Bulk Upload - Should delegate to SaveFilesAsync")]
    public async Task UploadImages_MultipleFiles_ShouldCallSaveFilesAsync()
    {
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload("img1.webp", ".webp", 1024)));
        var handler = new UploadManyProductImagesCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream1 = new MemoryStream();
        var stream2 = new MemoryStream();
        var command = new UploadManyProductImagesCommand
        {
            Files =
                [new FileParameter { Content = stream1, FileName = "test1.jpg" }, new FileParameter
                {
                    Content = stream2,
                    FileName = "test2.png"
                }]
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Value.Should().HaveCount(2);
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact(DisplayName = "MF_043 - Single Upload - Should delegate to SaveFileAsync")]
    public async Task UploadImage_ShouldCallSaveFileAsync()
    {
        var expectedStoragePath = "compressed-image.webp";
        var expectedFileExtension = ".webp";
        _fileInsertServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var largeStream = new MemoryStream(new byte[5242880]);
        var command = new UploadProductImageCommand { FileContent = largeStream, FileName = "original.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);
        _fileInsertServiceMock.Verify(
            x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(), It.IsAny<string>()),
            Times.Once());
    }

    [Fact(DisplayName = "MF_048 - Upload file vá»›i null stream")]
    public async Task UploadImage_NullStream_Fail()
    {
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var command = new UploadProductImageCommand { FileContent = null, FileName = "test.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Be("File is empty or required");
    }

    [Fact(DisplayName = "MF_049 - Upload file vá»›i empty stream")]
    public async Task UploadImage_EmptyStream_Fail()
    {
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var emptyStream = new MemoryStream([]);
        var command = new UploadProductImageCommand { FileContent = emptyStream, FileName = "test.webp" };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("empty");
    }

    [Fact(DisplayName = "MF_050 - Upload file vá»›i FileName rá»—ng")]
    public async Task UploadImage_EmptyFileName_Fail()
    {
        var handler = new UploadProductImageCommandHandler(
            _fileReadServiceMock.Object,
            _fileInsertServiceMock.Object,
            _insertRepositoryMock.Object,
            _unitOfWorkMock.Object);
        var stream = new MemoryStream(new byte[10]);
        var command = new UploadProductImageCommand { FileContent = stream, FileName = string.Empty };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("Filename");
    }

    [Fact(DisplayName = "MF_051 - URL file cÃ´ng khai khÃ´ng gáº¯n cá»©ng host lÃºc upload")]
    public void GetPublicUrl_ShouldReturnHostIndependentApiPath()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ContentRootPath).Returns(Path.GetTempPath());
        environment.SetupGet(x => x.WebRootPath).Returns(Path.Combine(Path.GetTempPath(), "wwwroot"));

  var service = new FileReadService(environment.Object,
    Options.Create(new LocalFileStorageOptions()),
    _fileUpdateServiceMock.Object,
    _httpContextAccessorMock.Object);
        var publicUrl = service.GetPublicUrl("banners/banner.webp");

        publicUrl.Should().Be("/api/v1/MediaFile/view-image/banners/banner.webp");
    }

    [Fact(DisplayName = "MF_052 - ÄÆ°á»ng dáº«n tÆ°Æ¡ng Ä‘á»‘i luÃ´n náº±m dÆ°á»›i content root cá»§a WebAPI")]
    public async Task SaveFile_RelativeUploadPath_ShouldResolveFromContentRoot()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"anhem-banner-{Guid.NewGuid():N}");
        var configuredUploadPath = Path.Combine("wwwroot", "uploads");
        var expectedUploadRoot = Path.Combine(contentRoot, configuredUploadPath);
        Directory.CreateDirectory(contentRoot);
        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(contentRoot);
            environment.SetupGet(x => x.WebRootPath).Returns(Path.Combine(contentRoot, "wwwroot"));
            await using var imageStream = new MemoryStream();
            using (var image = new Image<Rgba32>(2, 2))
            {
                await image.SaveAsPngAsync(imageStream, TestContext.Current.CancellationToken)
                    .ConfigureAwait(true);
            }
            imageStream.Position = 0;
            _fileUpdateServiceMock
                .Setup(
                    x => x.CompressImageAsync(
                        It.IsAny<Stream>(),
                        It.IsAny<int>(),
                        It.IsAny<int?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    () =>
                    {
                        imageStream.Position = 0;
                        return new MemoryStream(imageStream.ToArray());
                    });

  var service = new FileInsertService(environment.Object,
    Options.Create(new LocalFileStorageOptions { UploadPath = configuredUploadPath }),
    _fileUpdateServiceMock.Object);
            var result = await service.SaveFileAsync(
                    imageStream,
                    TestContext.Current.CancellationToken,
                    "banners")
                .ConfigureAwait(true);

            result.IsSuccess.Should().BeTrue();
            var expectedFile = Path.Combine(
                expectedUploadRoot,
                result.Value.StoragePath.Replace('/', Path.DirectorySeparatorChar));
            System.IO.File.Exists(expectedFile).Should().BeTrue();
        } finally
        {
            if (Directory.Exists(contentRoot))
            {
                Directory.Delete(contentRoot, true);
            }
        }
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
