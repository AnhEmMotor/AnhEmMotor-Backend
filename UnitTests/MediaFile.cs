using Application.ApiContracts.File.Requests;
using Application.ApiContracts.File.Responses;
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
using Moq;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace UnitTests;

public class MediaFile
{
    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    private readonly Mock<IFileReadService> _fileReadServiceMock;
    private readonly Mock<IFileInsertService> _fileInsertServiceMock;
    private readonly Mock<IFileUpdateService> _fileUpdateServiceMock;
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
        _fileDeleteServiceMock = new Mock<IFileDeleteService>();
        _insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        _readRepositoryMock = new Mock<IMediaFileReadRepository>();
        _updateRepositoryMock = new Mock<IMediaFileUpdateRepository>();
        _deleteRepositoryMock = new Mock<IMediaFileDeleteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "MF_001 - Tải lên ảnh thành công với định dạng WEBP hợp lệ")]
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

    [Fact(DisplayName = "MF_003 - Tải lên ảnh thất bại với định dạng không được hỗ trợ")]
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

    [Fact(DisplayName = "MF_004 - Tải lên ảnh thất bại khi kích thước file vượt quá giới hạn")]
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

    [Fact(DisplayName = "MF_008 - Tải lên nhiều ảnh thất bại khi có 1 file không hợp lệ (Bulk Request Rule)")]
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

    [Fact(DisplayName = "MF_010 - Xoá file thất bại khi file không tồn tại")]
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

    [Fact(DisplayName = "MF_011 - Xoá file thất bại khi file đã bị xoá trước đó")]
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

    [Fact(DisplayName = "MF_014 - Xoá nhiều file thất bại khi có 1 file không tồn tại (Bulk Request Rule)")]
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

    [Fact(DisplayName = "MF_016 - Khôi phục file thất bại khi file không tồn tại")]
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

    [Fact(DisplayName = "MF_017 - Khôi phục file thất bại khi file chưa bị xoá")]
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

    [Fact(DisplayName = "MF_020 - Khôi phục nhiều file thất bại khi có 1 file không tồn tại (Bulk Request Rule)")]
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

    [Fact(DisplayName = "MF_023 - Xem ảnh thất bại khi file không tồn tại")]
    public async Task ViewImage_FileNotFound_Fail()
    {
        _fileReadServiceMock.Setup(x => x.GetFileAsync("nonexistent.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "nonexistent.webp" };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_024 - Xem ảnh thất bại khi file đã bị xoá")]
    public async Task ViewImage_FileDeleted_Fail()
    {
        _fileReadServiceMock.Setup(x => x.GetFileAsync("deleted-image.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "deleted-image.webp" };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_025 - Xem ảnh thất bại khi width là số âm")]
    public async Task ViewImage_NegativeWidth_Fail()
    {
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "test.webp", Width = -100 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_026 - Xem ảnh thất bại khi width vượt quá giới hạn cho phép")]
    public async Task ViewImage_WidthExceedsLimit_Fail()
    {
        var handler = new ViewImageQueryHandler(_fileReadServiceMock.Object);
        var query = new ViewImageQuery { StoragePath = "test.webp", Width = 50000 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_027 - Validate: Tên file gốc chứa ký tự đặc biệt")]
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

    [Fact(DisplayName = "MF_028 - Validate: Tên file gốc có khoảng trắng đầu cuối")]
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

    [Fact(DisplayName = "MF_029 - Security: File signature không khớp với extension (webp fake)")]
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

    [Fact(DisplayName = "MF_030 - Security: File signature không khớp với extension (jpg fake)")]
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

    [Fact(DisplayName = "MF_032 - Lấy thông tin file theo ID thất bại khi file không tồn tại")]
    public async Task GetFileById_FileNotFound_Fail()
    {
        _readRepositoryMock.Setup(x => x.GetByIdAsync(999999, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new GetFileByIdQueryHandler(_readRepositoryMock.Object, _fileReadServiceMock.Object);
        var query = new GetFileByIdQuery { Id = 999999 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_033 - Lấy thông tin file theo ID thất bại khi file đã bị xoá")]
    public async Task GetFileById_FileDeleted_Fail()
    {
        _readRepositoryMock.Setup(x => x.GetByIdAsync(456, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);
        var handler = new GetFileByIdQueryHandler(_readRepositoryMock.Object, _fileReadServiceMock.Object);
        var query = new GetFileByIdQuery { Id = 456 };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "MF_040 - StorageType validation: Kiểm tra giá trị hợp lệ")]
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
            Files = [
                new FileParameter { Content = stream1, FileName = "test1.jpg" },
                new FileParameter { Content = stream2, FileName = "test2.png" }
            ]
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

    [Fact(DisplayName = "MF_048 - Upload file với null stream")]
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

    [Fact(DisplayName = "MF_049 - Upload file với empty stream")]
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

    [Fact(DisplayName = "MF_050 - Upload file với FileName rỗng")]
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
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}